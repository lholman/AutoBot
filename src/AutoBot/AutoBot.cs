using jabber;
using jabber.protocol.client;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Management.Automation;
using log4net;
using System.Linq;

namespace AutoBot
{
    public static class AutoBot
    {

        private static readonly AutoBot.HipChat.Session Session = new AutoBot.HipChat.Session();
        private static readonly PowerShellRunner PowershellRunner;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AutoBot.Cmd.Program));
                
        static AutoBot()
        {
            
            Session.Server = ConfigurationManager.AppSettings["HipChatServer"];
            Session.UserName = ConfigurationManager.AppSettings["HipChatUsername"];
            Session.Password = ConfigurationManager.AppSettings["HipChatPassword"];
            Session.Resource = ConfigurationManager.AppSettings["HipChatResource"];
            Session.MentionName = ConfigurationManager.AppSettings["HipChatBotMentionName"];
            Session.NickName = ConfigurationManager.AppSettings["HipChatBotNickName"];
            Session.SubscribedRooms = ConfigurationManager.AppSettings["HipChatRooms"];
            Session.OnMessageReceived += Session_OnMessageReceived;
                       
            PowershellRunner = new PowerShellRunner();            
        }

        public static void SetupChatConnection()
        {
            Session.Connect();
        }

        private static void Session_OnMessageReceived(object sender, Message message)
        {
            if (message.Body == null && message.X == null)
                return;

            string chatText = message.Body == null ? message.X.InnerText.Trim() : message.Body.Trim();

            if (string.IsNullOrEmpty(chatText) || chatText == " ")
                return;
            
            JID responseJid = new JID(message.From.User, message.From.Server, message.From.Resource);
            
            // intercept a handful of messages not directly for AutoBot
            if (message.Type == MessageType.groupchat && !chatText.Trim().StartsWith(Session.MentionName))
            {
                chatText = RemoveMentionFromMessage(chatText);
                SendRandomResponse(responseJid, chatText, message.Type);
                return;
            }

            // ensure the message is intended for AutoBot
            chatText = RemoveMentionFromMessage(chatText);
            PowerShellCommand powerShellCommand = BuildPowerShellCommand(chatText);
            Collection<PSObject> psObjects = PowershellRunner.RunPowerShellModule(powerShellCommand.CommandText,
                                                                            powerShellCommand.ParameterText);
            SendResponse(responseJid, psObjects, message.Type);
        }

        private static string RemoveMentionFromMessage(string chatText)
        {
            //TODO: Remove all @'s
            return chatText.Replace(Session.MentionName, string.Empty).Trim();
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
                
                Session.SendMessage(messageType, replyTo, message);
            }
        }

        private static void SendRandomResponse(JID replyTo, string chatText, MessageType messageType)
        {
            string[] chatTextWords = chatText.Split(' ');
            string message = string.Empty;
            switch (chatTextWords[0])
            {
                case "coolio":
                case "superb":
                    message = "Get-RandomImage " + chatText;
                    break;
                default:
                    break;
            }

            if (message != string.Empty)
            {
                PowerShellCommand powerShellCommand = BuildPowerShellCommand(message);
                Collection<PSObject> psObjects = PowershellRunner.RunPowerShellModule(powerShellCommand.CommandText,
                                                                                powerShellCommand.ParameterText);
                SendResponse(replyTo, psObjects, messageType);
            }
            return;
        }


    }
}
