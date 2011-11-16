using System.Collections;

namespace AutoBot.Cmd
{
    public class CommandLine
    {
        public Command GetCommand { get; private set; }

        public CommandLine(ArrayList args)
        {
            string command = args[0].ToString();
            string parameters = string.Empty;

            for (int i = 1; i < args.Count; i++)
            {
                parameters += args[i].ToString() + " ";
            }

            GetCommand = new Command(command, parameters);
        }

        public static bool CheckCommandLine(CommandLine commandLine)
        {
            //Add commandline checking, rules?

            return true;
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
