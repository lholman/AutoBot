using System.Linq;

namespace WinBot.Cmd
{
    public class CommandLine
    {
        public Command GetCommand { get; private set; }

        public CommandLine(string[] args)
        {
            GetCommand = args.Count() == 1 ? new Command(args[0], string.Empty) : new Command(args[0], args[1]);
        }

        public enum ExitCode : int
        {
            Success = 0,
            CommandLineError = 1,
            MissingResourceFile = 2,
            UnknownError = 10
        }

    }

    public class Command
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CommandText { get; set; }
        public string ParameterText { get; set; }

        public Command(string scriptName, string parameter)
        {
            CommandText = scriptName;
            ParameterText = parameter;
        }
    }
}
