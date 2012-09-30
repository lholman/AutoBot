using System;
using System.ServiceProcess;
using log4net;

namespace AutoBot
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        
        private static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("service", StringComparison.CurrentCultureIgnoreCase))
            {
                ServiceBase.Run(new ServiceBase[] { new Service() });
            }
            else
            {
                Logger.Info("Starting Autbot in console mode");
                try
                {
                    BotEngine botEngine = new BotEngine();
                    botEngine.Connect();
                }
                catch (Exception ex)
                {
                    Logger.Error("ERROR!:", ex);
                }
            }

        }

     }
}
