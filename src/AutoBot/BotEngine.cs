using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using jabber;
using jabber.client;
using jabber.connection;
using jabber.protocol;
using jabber.protocol.client;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Management.Automation;
using jabber.protocol.iq;
using log4net;
using System.Linq;

namespace AutoBot
{
    public class BotEngine : MarshalByRefObject
    {
        private readonly PowerShellRunner _powershellRunner;
        private Thread _thread;
        private bool _serviceStarted = false;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BotEngine));

        public delegate void MessageReceivedHandler(object sender, Message msg);
        public event MessageReceivedHandler OnMessageReceived;
        private JabberClient _jabberClient;
        private DiscoManager _discoManager;
        private PresenceManager _presenceManager;
        private ConferenceManager _conferenceManager;
        private string _mentionName;
        private string _subscribedRooms;
                
        public BotEngine()
        {
            _jabberClient = new JabberClient
            {

                Server = ConfigurationManager.AppSettings["HipChatServer"],
                User = ConfigurationManager.AppSettings["HipChatUsername"],
                Password = ConfigurationManager.AppSettings["HipChatPassword"],
                Resource = ConfigurationManager.AppSettings["HipChatResource"],
                AutoStartTLS = true,
                PlaintextAuth = true,
                AutoPresence = true,
                AutoRoster = false,
                AutoReconnect = 1,
                AutoLogin = true,
                KeepAlive = 10

            };
            
            _jabberClient.OnConnect += jabber_OnConnect;
            _jabberClient.OnAuthenticate += jabber_OnAuthenticate;
            _jabberClient.OnInvalidCertificate += jabber_OnInvalidCertificate;
            _jabberClient.OnError += jabber_OnError;
            _jabberClient.OnReadText += jabber_OnReadText;
            _jabberClient.OnWriteText += jabber_OnWriteText;
            _jabberClient.OnStreamInit += jabber_OnStreamInit;
            _jabberClient.OnDisconnect += jabber_OnDisconnect;
            _jabberClient.OnRegistered += jabber_OnRegistered;
            _jabberClient.OnRegisterInfo += jabber_OnRegisterInfo;
            _jabberClient.OnMessage += jabber_OnMessage;
            OnMessageReceived += Session_OnMessageReceived;

            _presenceManager = new PresenceManager
            {
                Stream = _jabberClient
            };
            _presenceManager.OnPrimarySessionChange += presenceManager_OnPrimarySessionChange;
            
            _conferenceManager = new ConferenceManager();
            _discoManager = new DiscoManager();

            _mentionName = ConfigurationManager.AppSettings["HipChatBotMentionName"];
            _subscribedRooms = ConfigurationManager.AppSettings["HipChatRooms"];
            _powershellRunner = new PowerShellRunner();
            
            _thread = new Thread(delegate()
                                     {
                                         while (_serviceStarted)
                                         {
                                             //TODO Add background task implementation here
                                             Thread.Sleep(1000);
                                         }
                                         Thread.CurrentThread.Abort();
                                     }
            );

        }

        public void Connect()
        {
            Logger.Info(string.Format("Connecting to '{0}'", _jabberClient.Server));
            _jabberClient.Connect();

            //Continue to retry connecting
            int retryCountLimit = 10;
            while (!_jabberClient.IsAuthenticated && retryCountLimit > 0)
            {
                retryCountLimit--;
                Thread.Sleep(1000);
            }

            //Successfully authenticated
            if (_jabberClient.IsAuthenticated)
            {
                Logger.Info(string.Format("Authenticated as '{0}'", _jabberClient.User));
                _serviceStarted = true;
                _thread.Start();
            }
        }

        public void Disconnect()
        {
            Logger.Info(string.Format("Disconnecting from '{0}'", _jabberClient.Server));
            _jabberClient.Close();
            _serviceStarted = false;
            _thread.Join(5000);
        }

        private void Session_OnMessageReceived(object sender, Message message)
        {
            if (message.Body == null && message.X == null)
                return;

            string chatText = message.Body == null ? message.X.InnerText.Trim() : message.Body.Trim();

            if (string.IsNullOrEmpty(chatText) || chatText == " ")
                return;
            
            JID responseJid = new JID(message.From.User, message.From.Server, message.From.Resource);
            
            // intercept a handful of messages not directly for AutoBot
            if (message.Type == MessageType.groupchat && !chatText.Trim().StartsWith(_mentionName))
            {
                chatText = RemoveMentionFromMessage(chatText);
                SendRandomResponse(responseJid, chatText, message.Type);
                return;
            }

            // ensure the message is intended for AutoBot
            chatText = RemoveMentionFromMessage(chatText);
            PowerShellCommand powerShellCommand = BuildPowerShellCommand(chatText);
            Collection<PSObject> psObjects = _powershellRunner.RunPowerShellModule(powerShellCommand.CommandText,
                                                                            powerShellCommand.ParameterText);
            SendResponse(responseJid, psObjects, message.Type);
        }

        private string RemoveMentionFromMessage(string chatText)
        {
            //TODO: Remove all @'s
            return chatText.Replace(_mentionName, string.Empty).Trim();
        }

        private static PowerShellCommand BuildPowerShellCommand(string chatText)
        {
            string[] chatTextArgs = chatText.Split(' ');
            string command = string.Empty;
            string parameters = string.Empty;

            command = chatTextArgs[0];

            for (int i = 0 + 1; i < chatTextArgs.Count(); i++)
                parameters += chatTextArgs[i] + " ";

            return new PowerShellCommand(command, parameters);
        }

        private void SendResponse(JID replyTo, Collection<PSObject> psObjects, MessageType messageType)
        {
            foreach (var psObject in psObjects)
            {
                Logger.Info(psObject.ImmediateBaseObject.GetType().FullName);
                string message = string.Empty;
                
                // the PowerShell (.NET) return types we are supporting
                if (psObject.BaseObject.GetType() == typeof(string))
                    message = psObject.ToString();
                
                else if (psObject.BaseObject.GetType() == typeof(Hashtable))
                {
                    Hashtable hashTable = (Hashtable)psObject.BaseObject;

                    foreach (DictionaryEntry dictionaryEntry in hashTable)
                        message += string.Format("{0} = {1}\n", dictionaryEntry.Key, dictionaryEntry.Value);
                }
                
                SendMessage(messageType, replyTo, message);
            }
        }

        private void SendRandomResponse(JID replyTo, string chatText, MessageType messageType)
        {
            string[] chatTextWords = chatText.Split(' ');
            string message = string.Empty;
            switch (chatTextWords[0])
            {
                case "coolio":
                case "gaytroll":
                    message = "Get-RandomImage " + chatText;
                    break;
                default:
                    break;
            }

            if (message != string.Empty)
            {
                PowerShellCommand powerShellCommand = BuildPowerShellCommand(message);
                Collection<PSObject> psObjects = _powershellRunner.RunPowerShellModule(powerShellCommand.CommandText,
                                                                                powerShellCommand.ParameterText);
                SendResponse(replyTo, psObjects, messageType);
            }
            return;
        }


        private void jabber_OnMessage(object sender, Message msg)
        {
            Logger.Debug(string.Format("RECV From: '{0}@{1}' : '{2}'", msg.From.User, msg.From.Server, msg.Body));
            this.OnMessageReceived(this, msg);
        }

        private bool jabber_OnRegisterInfo(object sender, Register register)
        {
            return true;
        }

        private void jabber_OnRegistered(object sender, IQ iq)
        {
            _jabberClient.Login();
        }

        private void jabber_OnDisconnect(object sender)
        {
            Logger.Info(string.Format("Disconnected from '{0}'", _jabberClient.Server));

        }

        private void jabber_OnStreamInit(object o, ElementStream elementStream)
        {
            var client = (JabberClient)o;
            _discoManager.Stream = client;
            _conferenceManager.Stream = client;
        }

        private void jabber_OnConnect(object o, StanzaStream s)
        {
            Logger.Info(string.Format("Connected to '{0}'", _jabberClient.Server));
        }

        private void jabber_OnAuthenticate(object o)
        {
            Logger.Info(string.Format("Authenticated to '{0}' as '{1}'", _jabberClient.Server, _jabberClient.User));
            _discoManager.BeginFindServiceWithFeature(URI.MUC, hlp_DiscoHandler_FindServiceWithFeature, new object());
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
                Logger.Debug("SEND: Keep alive");
                return;
            }
            Logger.Debug(string.Format("SEND: {0}", text));
        }

        private void hlp_DiscoHandler_FindServiceWithFeature(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            if (node.Name == "Rooms")
                _discoManager.BeginGetItems(node, hlp_DiscoHandler_SubscribeToRooms, new object());
        }

        private void hlp_DiscoHandler_SubscribeToRooms(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;

            if (node.Children != null && _subscribedRooms == "@all")
            {
                foreach (DiscoNode dn in node.Children)
                {
                    Logger.Info(string.Format("Subscribing to: '{0}':'{1}' on '{2}'", dn.JID, dn.Name, _jabberClient.Server));
                    // we have to build a new JID here, with the nickname included http://xmpp.org/extensions/xep-0045.html#enter-muc
                    JID subscriptionJid = new JID(dn.JID.User, dn.JID.Server, "AutoBot .");
                    Room room = _conferenceManager.GetRoom(subscriptionJid);
                    room.Join();
                }
            }
        }

        private void presenceManager_OnPrimarySessionChange(object sender, JID bare)
        {
            if (bare.Bare.Equals(_jabberClient.JID.Bare, StringComparison.InvariantCultureIgnoreCase))
                return;
        }

        public void SendMessage(MessageType messageType, string replyTo, string message)
        {
            _jabberClient.Message(messageType, replyTo, message);
        }

    }
}
