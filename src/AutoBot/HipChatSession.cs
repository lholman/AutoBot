using jabber;
using jabber.client;
using jabber.connection;
using jabber.protocol;
using jabber.protocol.client;
using jabber.protocol.iq;
using log4net;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace AutoBot
{
    internal class HipChatSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HipChatSession));
        
        public delegate void MessageReceivedHandler(object sender, Message msg);
        public event MessageReceivedHandler OnMessageReceived;

        private JabberClient _mJabberClient;
        private DiscoManager _mDiscoManager;
        private PresenceManager _mPresenceManager;
        private ManualResetEvent _mWaiter;
        private ConferenceManager _mConferenceManager;

        public string Server {get; set;}
        public string UserName {get; set;}
        public string Password {get; set;}
        public string Resource {get; set;}
        public string MentionName {get; set;}
        public string NickName {get; set;}
        public string SubscribedRooms {get; set;}

        #region JabberClient Event Handlers

        private void jabber_OnMessage(object sender, Message msg)
        {
            Logger.Debug(string.Format("RECV From: {0}@{1} : {2}", msg.From.User, msg.From.Server, msg.Body));
            this.OnMessageReceived(this, msg);
        }

        private bool jabber_OnRegisterInfo(object sender, Register register)
        {
            return true;
        }

        private void jabber_OnRegistered(object sender, IQ iq)
        {
            _mJabberClient.Login();
        }

        private static void jabber_OnDisconnect(object sender)
        {
            Logger.Info("Disconnecting");

        }

        private void jabber_OnStreamInit(object o, ElementStream elementStream)
        {
            var client = (JabberClient)o;
            _mDiscoManager.Stream = client;
            _mConferenceManager.Stream = client;
        }

        private void jabber_OnConnect(object o, StanzaStream s)
        {
            Logger.Info("Connecting");
            var client = (JabberClient)o;
        }

        private void jabber_OnAuthenticate(object o)
        {
            Logger.Info("Authenticated");
            _mDiscoManager.BeginFindServiceWithFeature(URI.MUC, hlp_DiscoHandler_FindServiceWithFeature, new object());
        }

        private bool jabber_OnInvalidCertificate(object o, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            // the current jabber server has an invalid certificate,
            // but override validation and accept it anyway.
            Logger.Info("Validating certificate");
            return true;
        }

        private void jabber_OnError(object o, Exception ex)
        {
            Logger.Error("ERROR!:", ex);
            throw ex;
        }

        private void jabber_OnReadText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ")
            {
                Logger.Debug("RECV: Keep alive");
                return;
            }
            Logger.Debug(string.Format("RECV: {0}", text));
        }

        private void jabber_OnWriteText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ")
            {
                Logger.Debug("RECV: Keep alive");
                return;
            }
            Logger.Debug(string.Format("SEND: {0}", text));
        }

        #endregion

        #region DiscoHandler Event Handlers

        private void hlp_DiscoHandler_FindServiceWithFeature(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            if (node.Name == "Rooms")
                _mDiscoManager.BeginGetItems(node, hlp_DiscoHandler_SubscribeToRooms, new object());
        }

        private void hlp_DiscoHandler_SubscribeToRooms(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            
            if (node.Children != null && SubscribedRooms == "@all")
            {
                foreach (DiscoNode dn in node.Children)
                {
                    Logger.Info(string.Format("Subscribing to: {0}:{1}", dn.JID, dn.Name));
                    // we have to build a new JID here, with the nickname included http://xmpp.org/extensions/xep-0045.html#enter-muc
                    JID subscriptionJid = new JID(dn.JID.User, dn.JID.Server, "AutoBot .");
                    Room room = _mConferenceManager.GetRoom(subscriptionJid);
                    room.Join();
                }
            }
        }

        #endregion

        #region PresenceManager Event Handlers

        private void presenceManager_OnPrimarySessionChange(object sender, JID bare)
        {
            if (bare.Bare.Equals(_mJabberClient.JID.Bare, StringComparison.InvariantCultureIgnoreCase))
                return;
        }

        #endregion

        #region Methods

        public void Connect()
        {
            _mJabberClient = new JabberClient
            {
                Server = this.Server,
                User = this.UserName,
                Password = this.Password,
                Resource = this.Resource,
                AutoStartTLS = true,
                PlaintextAuth = true,
                AutoPresence = true,
                AutoRoster = false,
                AutoReconnect = 1,
                AutoLogin = true,
                KeepAlive = 10
                
            };

            _mPresenceManager = new PresenceManager
            {
                Stream = _mJabberClient
            };

            _mConferenceManager = new ConferenceManager();

            _mDiscoManager = new DiscoManager();
            _mWaiter = new ManualResetEvent(false);
            _mPresenceManager.OnPrimarySessionChange += presenceManager_OnPrimarySessionChange;
            _mJabberClient.OnConnect += jabber_OnConnect;
            _mJabberClient.OnAuthenticate += jabber_OnAuthenticate;
            _mJabberClient.OnInvalidCertificate += jabber_OnInvalidCertificate;
            _mJabberClient.OnError += jabber_OnError;
            _mJabberClient.OnReadText += jabber_OnReadText;
            _mJabberClient.OnWriteText += jabber_OnWriteText;
            _mJabberClient.OnStreamInit += jabber_OnStreamInit;
            _mJabberClient.OnDisconnect += jabber_OnDisconnect;
            _mJabberClient.OnRegistered += jabber_OnRegistered;
            _mJabberClient.OnRegisterInfo += jabber_OnRegisterInfo;
            _mJabberClient.OnMessage += jabber_OnMessage;
            // connect. this is synchronous so we'll use a manual reset event
            // to pause this thread forever. client events will continue to
            // fire but we won't have to worry about setting up an idle "while" loop.
            _mJabberClient.Connect();
            _mWaiter.WaitOne();
        }

        public void SendMessage(MessageType messageType, string replyTo, string message)
        {
            _mJabberClient.Message(messageType, replyTo, message);
        }

        #endregion

    }

}
