using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using jabber.client;
using jabber.connection;

namespace AutoBot.Cmd
{
    public class HipChat
    {
        private static readonly ManualResetEvent done = new ManualResetEvent(false);
        private static string _hipChatServer;
        private static string _hipChatUsername;
        private static string _hipChatPassword; 
        private static string _hipChatResource;
        private static string _hipChatBotName;

        internal static void SetupChatConnection()
        {
            _hipChatServer = ConfigurationManager.AppSettings["HipChatServer"];
            _hipChatUsername = ConfigurationManager.AppSettings["HipChatUsername"];
            _hipChatPassword = ConfigurationManager.AppSettings["HipChatPassword"];
            _hipChatResource = ConfigurationManager.AppSettings["HipChatResource"];
            _hipChatBotName = ConfigurationManager.AppSettings["HipChatBotName"];

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

            // connect. this is synchronous so we'll use a manual reset event
            // to pause this thread forever. client events will continue to
            // fire but we won't have to worry about setting up an idle "while" loop.
            client.Connect();
            done.WaitOne();
        }

        private static void jabber_OnConnect(object o, StanzaStream s)
        {
            Console.WriteLine("connecting");
        }

        private static void jabber_OnAuthenticate(object o)
        {
            Console.WriteLine("authenticated");
            var client = (JabberClient)o;
        }

        private static bool jabber_OnInvalidCertificate(object o, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            // the current jabber server has an invalid certificate,
            // but override validation and accept it anyway.
            Console.WriteLine("validating certificate");
            return true;
        }

        private static void jabber_OnError(object o, Exception e)
        {
            Console.WriteLine("error occurred");
            Console.WriteLine(e.ToString());
            throw e;
        }

        private static void jabber_OnReadText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ") return;
            Console.WriteLine("RECV: " + text);
            // check if this is an incoming message for autobot
            if (text.StartsWith("<message "))
            {
                ProcessRequest((JabberClient)sender, text);
            }
        }

        private static void jabber_OnWriteText(object sender, string text)
        {
            // ignore keep-alive spaces
            if (text == " ") return;
            Console.WriteLine("SEND: " + text);
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

            //Strip off the @autobot from the chatText
            string[] chatTextArgs = chatText.Split(' ');
            ArrayList arrayList = new ArrayList();
            for (int i = 1; i < chatTextArgs.Count(); i++)
                arrayList.Add(chatTextArgs[i]);

            CommandLine commandLine = new CommandLine(arrayList);
            CommandLine.CheckCommandLine(commandLine);
            
            Command command = commandLine.GetCommand;

            string responseText = Powershell.RunPowershellModule(command.CommandText, command.ParameterText);
            SendResponse(client, replyTo, responseText);
        }

        private static void SendResponse(JabberClient client, string replyTo, string text)
        {
            string message = string.Empty;

            if (text.StartsWith("@{") && text.EndsWith("}"))
            {
                //assume we are passing a PowerShell hash table result
                text = text.Substring(2, text.Length - 3);
                string[] rows = text.Split(';');
                message = rows.Aggregate(message, (current, row) => current + row.Trim());
            }
            else
                message = text;

            // build the reply message
            var template = "<message to=\"\" type=\"chat\" from=\"\"><body></body></message>";
            var response = new XmlDocument();
            response.LoadXml(template);
            response.SelectSingleNode("message/@to").InnerText = replyTo;
            response.SelectSingleNode("message/@from").InnerText = _hipChatUsername;
            response.SelectSingleNode("message/body").InnerText = message;
            // send the reply message
            client.Write(response.OuterXml);
        }


    }
}
