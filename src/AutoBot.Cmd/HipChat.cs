using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using jabber.client;
using jabber.connection;
using jabber.protocol;
using jabber.protocol.stream;
using log4net;

namespace AutoBot.Cmd
{
    public static class HipChat
    {
        private static readonly ManualResetEvent done = new ManualResetEvent(false);
        private static string _hipChatServer;
        private static string _hipChatUsername;
        private static string _hipChatPassword; 
        private static string _hipChatResource;
        private static string _hipChatBotName;
        private static string _hipChatRooms;
        private static readonly DiscoManager discoManager = new DiscoManager();
        private static XmppStream stream;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        public static void SetupChatConnection()
        {
            _hipChatServer = ConfigurationManager.AppSettings["HipChatServer"];
            _hipChatUsername = ConfigurationManager.AppSettings["HipChatUsername"];
            _hipChatPassword = ConfigurationManager.AppSettings["HipChatPassword"];
            _hipChatResource = ConfigurationManager.AppSettings["HipChatResource"];
            _hipChatBotName = ConfigurationManager.AppSettings["HipChatBotName"];
            _hipChatRooms = ConfigurationManager.AppSettings["HipChatRooms"];
            
            // set up the client connection details
            var client = new JabberClient();
            client.Server = _hipChatServer;
            client.User = _hipChatUsername;
            client.Password = _hipChatPassword;
            client.Resource = _hipChatResource;
            client.AutoStartTLS = true;
            client.PlaintextAuth = true;

            // set some other properties on the client connection
            client.AutoPresence = true;
            client.AutoRoster = false;
            client.AutoReconnect = -1;

            // set up some event handlers
            client.OnConnect += jabber_OnConnect;
            client.OnAuthenticate += jabber_OnAuthenticate;
            client.OnInvalidCertificate += jabber_OnInvalidCertificate;
            client.OnError += jabber_OnError;
            client.OnReadText += jabber_OnReadText;
            client.OnWriteText += jabber_OnWriteText;
            client.OnStreamInit += jabber_OnStreamInit;
            client.OnDisconnect += jabber_OnDisconnect;

            // connect. this is synchronous so we'll use a manual reset event
            // to pause this thread forever. client events will continue to
            // fire but we won't have to worry about setting up an idle "while" loop.
            client.Connect();
            done.WaitOne();
        }

        private static void jabber_OnDisconnect(object sender)
        {
            throw new NotImplementedException();

        }

        private static void jabber_OnStreamInit(object o, ElementStream elementStream)
        {
            var client = (JabberClient)o;
            discoManager.Stream = client;
             
        }

        private static void jabber_OnConnect(object o, StanzaStream s)
        {
            _logger.Info("Connecting");
            var client = (JabberClient) o;
        }

        private static void hlp_DiscoHandler_GetItems(DiscoManager sender, DiscoNode node, object state)
        {
            if (node.Children != null)
            {
                int x = node.Children.Count;
                foreach (jabber.connection.DiscoNode dn in node.Children)
                {

                    _logger.Info(dn.Name);
                    _logger.Info(dn.Node);
                    _logger.Info(dn.JID);
                }
            }
        }

        private static void jabber_OnAuthenticate(object o)
        {
            _logger.Info("Authenticated");
            var client = (JabberClient)o;
            string streamId = client.StreamID;
            DiscoNode discoNode = discoManager.GetNode(client.JID);

            //discoManager.Stream = stream;
            
            discoManager.BeginFindServiceWithFeature(URI.MUC, hlp_DiscoHandler_GetItems, new object());

            //if (_hipChatRooms == "@all")
        }

        private static bool jabber_OnInvalidCertificate(object o, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            // the current jabber server has an invalid certificate,
            // but override validation and accept it anyway.
            _logger.Info("Validating certificate");
            return true;
        }

        private static void jabber_OnError(object o, Exception ex)
        {
            _logger.Error("ERROR!:", ex);
            throw ex;
        }

        private static void jabber_OnReadText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ") return;
                _logger.Debug(string.Format("RECV: {0}", text));
            // check if this is an incoming message for autobot
            if (text.StartsWith("<message "))
                ProcessRequest((JabberClient)sender, text);
        }

        private static void jabber_OnWriteText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ") return;
                _logger.Debug(string.Format("SEND: {0}", text));
        }

        private static void ProcessRequest(JabberClient client, string message)
        {
            // parse the message xml to extract the text the user typed.
            // no_te that presence notifications also arrive as <message>
            // tags so we need to look specifically for a <body> to see
            // if it's a "real" message or not.
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(message);
            XmlNode body = dom.SelectSingleNode("message/body");
            if (body == null)
                return;

            // get the return address / room to post the response message to
            var replyTo = dom.SelectSingleNode("message/@from").InnerText;

            // work out what command was requested
            var chatText = body.InnerText.Trim();
            if (string.IsNullOrEmpty(chatText))
                return;

            if (!body.InnerText.Trim().StartsWith(string.Format("@{0} ", _hipChatBotName)))
                return;

            // strip off the @autobot from the chatText
            string[] chatTextArgs = chatText.Split(' ');
            ArrayList arrayList = new ArrayList();
            for (int i = 1; i < chatTextArgs.Count(); i++)
                arrayList.Add(chatTextArgs[i]);

            CommandLine commandLine = new CommandLine(arrayList);
            
            CommandLine.CheckCommandLine(commandLine);
            
            Command command = commandLine.GetCommand;
            Collection<PSObject> psObjects = Powershell.RunPowershellModule(command.CommandText, command.ParameterText);
            SendResponse(client, replyTo, psObjects);
        }

        private static void SendResponse(JabberClient client, string replyTo, Collection<PSObject> psObjects)
        {
            foreach (var psObject in psObjects)
            {
                _logger.Info(psObject.ImmediateBaseObject.GetType().FullName);
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

                const string template = "<message to=\"\" type=\"chat\" from=\"\"><body></body></message>";
                var response = new XmlDocument();
                response.LoadXml(template);
                response.SelectSingleNode("message/@to").InnerText = replyTo;
                response.SelectSingleNode("message/@from").InnerText = _hipChatUsername;
                response.SelectSingleNode("message/body").InnerText = message;
                client.Write(response.OuterXml);
            }
        }


    }
}
