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
            Logger.Info("Starting in service mode");
            _botEngine = new BotEngine();

            _botEngine.Connect();
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping from service mode");
            _botEngine.Disconnect();
        }
    }
}
