using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace WinBot.Cmd
{
    class Program
    {
        private static string _scriptsPath = "C:\\Development\\WinBot\\WinBot.Cmd\\Scripts\\";

        static int Main(string[] args)
        {
            Environment.ExitCode = (int)CommandLine.ExitCode.Success;
            try
            {
                CommandLine commandLine = new CommandLine(args);
                if (!CheckCommandLine(commandLine))
                {
                    Environment.ExitCode = (int)CommandLine.ExitCode.CommandLineError;
                    return Environment.ExitCode;
                }
                    Command command = commandLine.GetCommand;
                    RunPowershellModule(command.CommandText, command.ParameterText);
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

        private static void RunPowershellModule(string scriptName, string command)
        {
            using (RunspaceInvoke invoker = new RunspaceInvoke())
            {
                // load the powershell module
                string scriptPath = GetPath(scriptName);
                invoker.Invoke(string.Format("Import-Module {0}.psm1", scriptPath));

                // run the Function with the same name as the module 
                Collection<PSObject> report = invoker.Invoke(string.Format("{0} {1}", scriptName, command));
                ConsoleExtender.Write(report[0].ToString());
            }
        }

        private static void GetPowershellModuleHelp(string scriptName)
        {
            using (RunspaceInvoke invoker = new RunspaceInvoke())
            {
                // load the powershell module
                string scriptPath = GetPath(scriptName);
                invoker.Invoke(string.Format("Import-Module {0}.psm1", scriptPath));

                // get Powershell help for the function with the same name as the module 
                Collection<PSObject> report = invoker.Invoke(string.Format("Get-Help {0}", scriptName));
                ConsoleExtender.Write(report[0].ToString());
            }
        }

        private static string GetPath(string appendFile)
        {
            
            if (!(_scriptsPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) || _scriptsPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)))
                _scriptsPath += Path.DirectorySeparatorChar;

            return _scriptsPath + appendFile;
        }

        static bool CheckCommandLine(CommandLine commandLine)
        {
            if (commandLine.GetCommand.ParameterText == string.Empty)
            {
                switch (commandLine.GetCommand.CommandText)
                {
                    case "Get-Help":
                    case "-?":
                        return DisplayUsage();
                    default:
                        return DisplayUsage();
                }
            }

            switch (commandLine.GetCommand.CommandText)
            {
                case "Get-Help":
                case "-?":
                    return DisplayUsage(commandLine.GetCommand.ParameterText);
            }

            return true;
        }

        static bool DisplayUsage(string powershellModuleName = null)
        {
            if (powershellModuleName != null)
                GetPowershellModuleHelp(powershellModuleName);
            else
                ListPowershellModules(_scriptsPath);

            return false;
        }

        private static void ListPowershellModules(string path)
        {
            string[] subDirectories = Directory.GetDirectories(path);
            foreach (string subDirectory in subDirectories)
                ListPowershellModules(subDirectory);

            string[] files = Directory.GetFiles(path, "*.psm1");
            foreach (string file in files)
            {
                ConsoleExtender.Info(Path.GetFileNameWithoutExtension(file));
            }
        }
     }
}
