using System;

namespace AutoBot.Cmd
{
    class Program
    {

        private static int Main(string[] args)
        {
            Environment.ExitCode = (int)CommandLine.ExitCode.Success;
            try
            {
                HipChat.SetupChatConnection();
            }
            catch (Exception ex)
            {
                Environment.ExitCode = (int)CommandLine.ExitCode.UnknownError;

                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR!: {0}", ex.Message + ex.StackTrace);
                Console.ForegroundColor = old;
            }
            return Environment.ExitCode;
        }

     }
}
