using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading;

namespace AutoBot.Host
{

    public class AutoBotHost : PSHost
    {

        #region Fields

        private Guid m_InstanceId;
        private PSHostUserInterface m_UI;

        #endregion

        #region PSHost Members

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override Guid InstanceId
        {
            get
            {
                if (m_InstanceId == Guid.Empty)
                {
                    m_InstanceId = Guid.NewGuid();
                }
                return m_InstanceId;
            }
        }

        public override string Name
        {
            get
            {
                return "AutoBotHost";
            }
        }

        public override void NotifyBeginApplication()
        {
            throw new NotImplementedException();
        }

        public override void NotifyEndApplication()
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.Host.PSHostUserInterface UI
        {
            get
            {
                if(m_UI == null)
                {
                    m_UI = new AutoBotUserInterface();
                }
                return m_UI;
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }

        #endregion
    }

}
