using jabber;
using jabber.client;
using jabber.connection;
using jabber.protocol;
using jabber.protocol.client;
using jabber.protocol.iq;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace AutoBot.HipChat
{
    
    internal class Session
    {

        #region Events

        public delegate void MessageReceivedHandler(object sender, Message msg);
        public event MessageReceivedHandler OnMessageReceived;

        #endregion

        #region Fields

        private JabberClient m_JabberClient;
        private DiscoManager m_DiscoManager;
        private PresenceManager m_PresenceManager;

        private readonly ILog m_Logger = LogManager.GetLogger(typeof(AutoBot.Cmd.Program));
        private ManualResetEvent m_Waiter;

        #endregion

        #region Constructors

        public Session()
            : base()
        {
        }
        
        #endregion

        #region Properties

        public string Server
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string Resource
        {
            get;
            set;
        }

        public string MentionName
        {
            get;
            set;
        }

        public string NickName
        {
            get;
            set;
        }

        public string SubscribedRooms
        {
            get;
            set;
        }

        #endregion

        #region JabberClient Event Handlers

        private void jabber_OnMessage(object sender, Message msg)
        {
            m_Logger.Debug(string.Format("RECV From: {0}@{1} : {2}", msg.From.User, msg.From.Server, msg.Body));
            this.OnMessageReceived(this, msg);
        }

        private bool jabber_OnRegisterInfo(object sender, Register register)
        {
            return true;
        }

        private void jabber_OnRegistered(object sender, IQ iq)
        {
            m_JabberClient.Login();
        }

        private static void jabber_OnDisconnect(object sender)
        {
            throw new NotImplementedException();

        }

        private void jabber_OnStreamInit(object o, ElementStream elementStream)
        {
            var client = (JabberClient)o;
            m_DiscoManager.Stream = client;
        }

        private void jabber_OnConnect(object o, StanzaStream s)
        {
            m_Logger.Info("Connecting");
            var client = (JabberClient)o;
        }

        private void jabber_OnAuthenticate(object o)
        {
            m_Logger.Info("Authenticated");
            m_DiscoManager.BeginFindServiceWithFeature(URI.MUC, hlp_DiscoHandler_FindServiceWithFeature, new object());
        }

        private bool jabber_OnInvalidCertificate(object o, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            // the current jabber server has an invalid certificate,
            // but override validation and accept it anyway.
            m_Logger.Info("Validating certificate");
            return true;
        }

        private void jabber_OnError(object o, Exception ex)
        {
            m_Logger.Error("ERROR!:", ex);
            throw ex;
        }

        private void jabber_OnReadText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ")
            {
                m_Logger.Debug("RECV: Keep alive");
                return;
            }
            m_Logger.Debug(string.Format("RECV: {0}", text));
        }

        private void jabber_OnWriteText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ")
            {
                m_Logger.Debug("RECV: Keep alive");
                return;
            }
            m_Logger.Debug(string.Format("SEND: {0}", text));
        }

        #endregion

        #region DiscoHandler Event Handlers 

        private void hlp_DiscoHandler_FindServiceWithFeature(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            if (node.Name == "Rooms")
                m_DiscoManager.BeginGetItems(node, hlp_DiscoHandler_SubscribeToRooms, new object());
        }

        private void hlp_DiscoHandler_SubscribeToRooms(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            if (node.Children != null && this.SubscribedRooms == "@all")
            {
                foreach (DiscoNode dn in node.Children)
                {
                    m_Logger.Info(string.Format("Subscribing to: {0}:{1}", dn.JID, dn.Name));
                    // we have to build a new JID here, with the nickname included http://xmpp.org/extensions/xep-0045.html#enter-muc
                    JID subscriptionJid = new JID(dn.JID.User, dn.JID.Server, "AutoBot .");
                    m_JabberClient.Subscribe(subscriptionJid, this.NickName, null);
                }
            }
        }

        #endregion

        #region PresenceManager Event Handlers

        private  void presenceManager_OnPrimarySessionChange(object sender, JID bare)
        {
            if (bare.Bare.Equals(m_JabberClient.JID.Bare, StringComparison.InvariantCultureIgnoreCase))
                return;
        }

        #endregion

        #region Methods

        public void Connect()
        {
            m_JabberClient = new JabberClient
            {
                Server = this.Server,
                User = this.UserName,
                Password = this.Password,
                Resource = this.Resource,
                AutoStartTLS = true,
                PlaintextAuth = true,
                AutoPresence = true,
                AutoRoster = false,
                AutoReconnect = -1,
                AutoLogin = true
            };
            m_PresenceManager = new PresenceManager
            {
                Stream = m_JabberClient
            };
            m_DiscoManager = new DiscoManager();
            m_Waiter = new ManualResetEvent(false);
            m_PresenceManager.OnPrimarySessionChange += presenceManager_OnPrimarySessionChange;
            m_JabberClient.OnConnect += jabber_OnConnect;
            m_JabberClient.OnAuthenticate += jabber_OnAuthenticate;
            m_JabberClient.OnInvalidCertificate += jabber_OnInvalidCertificate;
            m_JabberClient.OnError += jabber_OnError;
            m_JabberClient.OnReadText += jabber_OnReadText;
            m_JabberClient.OnWriteText += jabber_OnWriteText;
            m_JabberClient.OnStreamInit += jabber_OnStreamInit;
            m_JabberClient.OnDisconnect += jabber_OnDisconnect;
            m_JabberClient.OnRegistered += jabber_OnRegistered;
            m_JabberClient.OnRegisterInfo += jabber_OnRegisterInfo;
            m_JabberClient.OnMessage += jabber_OnMessage;
            // connect. this is synchronous so we'll use a manual reset event
            // to pause this thread forever. client events will continue to
            // fire but we won't have to worry about setting up an idle "while" loop.
            m_JabberClient.Connect();
            m_Waiter.WaitOne();
        }

        public void SendMessage(MessageType messageType, string replyTo, string message)
        {
            m_JabberClient.Message(messageType, replyTo, message);
        }

        #endregion

    }

}
