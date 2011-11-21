using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Management.Automation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using jabber;
using jabber.client;
using jabber.connection;
using jabber.protocol;
using jabber.protocol.client;
using jabber.protocol.iq;
using log4net;

namespace AutoBot.Cmd
{
    public static class HipChat
    {
        private static readonly JabberClient JabberClient;
        private static readonly PresenceManager PresenceManager;
        private static readonly PowerShellRunner PowershellRunner;

        private static readonly ManualResetEvent Done;
        private static readonly DiscoManager DiscoManager;
        private static readonly ILog Logger;
        
        private static readonly string HipChatServer;
        private static readonly string HipChatUsername;
        private static readonly string HipChatPassword; 
        private static readonly string HipChatResource;
        private static readonly string HipChatBotName;
        private static readonly string HipChatRooms;
        
        static HipChat()
        {
            HipChatServer = ConfigurationManager.AppSettings["HipChatServer"];
            HipChatUsername = ConfigurationManager.AppSettings["HipChatUsername"];
            HipChatPassword = ConfigurationManager.AppSettings["HipChatPassword"];
            HipChatResource = ConfigurationManager.AppSettings["HipChatResource"];
            HipChatBotName = ConfigurationManager.AppSettings["HipChatBotName"];
            HipChatRooms = ConfigurationManager.AppSettings["HipChatRooms"];

            PresenceManager = new PresenceManager
                                   {
                                       Stream = JabberClient
                                   };
            
            PowershellRunner = new PowerShellRunner();
            Done = new ManualResetEvent(false);
            DiscoManager = new DiscoManager();
            Logger = LogManager.GetLogger(typeof(Program));
            
            JabberClient = new JabberClient
                                {
                                    Server = HipChatServer,
                                    User = HipChatUsername,
                                    Password = HipChatPassword,
                                    Resource = HipChatResource,
                                    AutoStartTLS = true,
                                    PlaintextAuth = true,
                                    AutoPresence = true,
                                    AutoRoster = false,
                                    AutoReconnect = -1,
                                    AutoLogin = true
                                };

            PresenceManager.OnPrimarySessionChange += _presenceManager_OnPrimarySessionChange;

            JabberClient.OnConnect += jabber_OnConnect;
            JabberClient.OnAuthenticate += jabber_OnAuthenticate;
            JabberClient.OnInvalidCertificate += jabber_OnInvalidCertificate;
            JabberClient.OnError += jabber_OnError;
            JabberClient.OnReadText += jabber_OnReadText;
            JabberClient.OnWriteText += jabber_OnWriteText;
            JabberClient.OnStreamInit += jabber_OnStreamInit;
            JabberClient.OnDisconnect += jabber_OnDisconnect;
            JabberClient.OnRegistered += jabber_OnRegistered;
            JabberClient.OnRegisterInfo += jabber_OnRegisterInfo;
            JabberClient.OnMessage += jabber_OnMessage;

        }

        private static void jabber_OnMessage(object sender, Message msg)
        {
            Logger.Debug(string.Format("RECV From: {0}@{1} : {2}", msg.From.User, msg.From.Server, msg.Body));
            ProcessRequest(msg);
        }

        private static bool jabber_OnRegisterInfo(object sender, Register register)
        {
            return true;
        }

        private static void jabber_OnRegistered(object sender, IQ iq)
        {
            JabberClient.Login();
        }

        private static void _presenceManager_OnPrimarySessionChange(object sender, JID bare)
        {
            if (bare.Bare.Equals(JabberClient.JID.Bare, StringComparison.InvariantCultureIgnoreCase))
                return;
        }

        public static void SetupChatConnection()
        {
            // connect. this is synchronous so we'll use a manual reset event
            // to pause this thread forever. client events will continue to
            // fire but we won't have to worry about setting up an idle "while" loop.
            JabberClient.Connect();
            Done.WaitOne();
        }

        private static void jabber_OnDisconnect(object sender)
        {
            throw new NotImplementedException();

        }

        private static void jabber_OnStreamInit(object o, ElementStream elementStream)
        {
            var client = (JabberClient)o;
            DiscoManager.Stream = client;
             
        }

        private static void jabber_OnConnect(object o, StanzaStream s)
        {
            Logger.Info("Connecting");
            var client = (JabberClient) o;
        }

        private static void hlp_DiscoHandler_FindServiceWithFeature(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            
            if (node.Name == "Rooms")
                DiscoManager.BeginGetItems(node, hlp_DiscoHandler_SubscribeToRooms, new object());
        }

        private static void hlp_DiscoHandler_SubscribeToRooms(DiscoManager sender, DiscoNode node, object state)
        {
            if (node == null)
                return;
            if (node.Children != null && HipChatRooms == "@all")
            {
                foreach (DiscoNode dn in node.Children)
                {
                    Logger.Info(string.Format("Subscribing to: {0}:{1}", dn.JID, dn.Name));
                    // we have to build a new JID here, with the nickname included http://xmpp.org/extensions/xep-0045.html#enter-muc
                    JID subscriptionJid = new JID(dn.JID.User, dn.JID.Server, "AutoBot .");
                    JabberClient.Subscribe(subscriptionJid, "AutoBot .", null);
                }
            }
        }

        private static void jabber_OnAuthenticate(object o)
        {
            Logger.Info("Authenticated");
            DiscoManager.BeginFindServiceWithFeature(URI.MUC, hlp_DiscoHandler_FindServiceWithFeature, new object());
        }

        private static bool jabber_OnInvalidCertificate(object o, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            // the current jabber server has an invalid certificate,
            // but override validation and accept it anyway.
            Logger.Info("Validating certificate");
            return true;
        }

        private static void jabber_OnError(object o, Exception ex)
        {
            Logger.Error("ERROR!:", ex);
            throw ex;
        }

        private static void jabber_OnReadText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ")
            {
                Logger.Debug("RECV: Keep alive");
                return;
            }
            Logger.Debug(string.Format("RECV: {0}", text));
        }

        private static void jabber_OnWriteText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ")
            {
                Logger.Debug("RECV: Keep alive");
                return;
            }
            Logger.Debug(string.Format("SEND: {0}", text));
        }

        private static void ProcessRequest(Message message)
        {
            if (message.Body == null && message.X == null)
                return;

            string chatText = message.Body == null ? message.X.InnerText.Trim() : message.Body.Trim();

            if (string.IsNullOrEmpty(chatText) || chatText == " ")
                return;

            //Ensure the message is intended for AutoBot
            if (message.Type == MessageType.groupchat && !chatText.StartsWith(string.Format("@{0} ", HipChatBotName)))
                return;

            PowershellRunner.BuildPowerShellCommand(chatText, HipChatBotName);
            PowerShellCommand powerShellCommand = PowershellRunner.GetPowerShellCommand;

            Collection<PSObject> psObjects = PowershellRunner.RunPowerShellModule(powerShellCommand.CommandText,
                                                                            powerShellCommand.ParameterText);
            JID responseJid = new JID(message.From.User, message.From.Server, message.From.Resource);
            SendResponse(responseJid, psObjects, message.Type);
        }

        private static void SendResponse(JID replyTo, Collection<PSObject> psObjects, MessageType messageType)
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
                
                JabberClient.Message(messageType, replyTo, message);
            }
        }


    }
}
