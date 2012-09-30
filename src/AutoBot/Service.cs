using System;
using System.ServiceProcess;
using log4net;

namespace AutoBot
{
    partial class Service : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Service));

        public Service()
        {
            InitializeComponent();
        }

        private BotEngine _botEngine;

        protected override void OnStart(string[] args)
        {
            Logger.Info("Starting AutoBot Windows service");
            
            _botEngine = new BotEngine();
            _botEngine.Connect();
        }


        protected override void OnStop()
        {
            Logger.Info("Stopping AutoBot Windows service");

            _botEngine.Disconnect();
        }
    }
}
