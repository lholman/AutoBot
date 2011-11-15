using System;
using log4net;

namespace AutoBot.Cmd
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        
        private static int Main(string[] args)
        {
            _logger.Debug("Debug statement");
            _logger.Info("Info statement");
            _logger.Error("Error statement");
            _logger.Fatal("Fatal statatement");

            Environment.ExitCode = (int)CommandLine.ExitCode.Success;
            try
            {
                HipChat.SetupChatConnection();
            }
            catch (Exception ex)
            {
                Environment.ExitCode = (int)CommandLine.ExitCode.UnknownError;
                _logger.Error("ERROR!:", ex);
            }
            return Environment.ExitCode;
        }

     }
}
