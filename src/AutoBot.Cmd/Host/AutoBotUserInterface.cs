using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using log4net;

namespace AutoBot.Host
{

    internal class AutoBotUserInterface: PSHostUserInterface
    {

        #region Fields

        private readonly ILog _logger = LogManager.GetLogger(typeof(AutoBot.Cmd.Program));

        private PSHostRawUserInterface m_RawUI;

        #endregion

        #region PSHostUserInterface Members

        #region Input Methods

        // it's a bot - we don't support input

        public override Dictionary<string, PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                if (m_RawUI == null)
                {
                    m_RawUI = new AutoBotRawUserInterface();
                }
                return m_RawUI;
            }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Script Output Methods

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _logger.Info(value);
        }

        public override void Write(string value)
        {
            this.Write(this.RawUI.ForegroundColor, this.RawUI.BackgroundColor, value);
        }

        public override void WriteLine(string value)
        {
            this.Write(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            this.Write(record.PercentComplete.ToString());
        }

        #endregion

        #region Logging Output Methods

        public override void WriteDebugLine(string message)
        {
            _logger.Debug(message);
        }

        public override void WriteErrorLine(string value)
        {
            _logger.Error(value);
        }

        public override void WriteVerboseLine(string message)
        {
            _logger.Info(message);
        }

        public override void WriteWarningLine(string message)
        {
            _logger.Warn(message);
        }

        #endregion

        #endregion

    }
        
}
