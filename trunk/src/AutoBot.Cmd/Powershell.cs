using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace AutoBot.Cmd
{
    public class Powershell
    {
        private static readonly string ScriptsPath = Path.Combine(Environment.CurrentDirectory, "Scripts");

        internal static string RunPowershellModule(string scriptName, string command)
        {
            
            using (RunspaceInvoke invoker = new RunspaceInvoke())
            {
                // load the powershell module
                string scriptPath = GetPath(scriptName);
                invoker.Invoke(string.Format("Import-Module {0}.psm1", scriptPath));

                Collection<PSObject> report;

                if (scriptName == "Get-Help" && command == string.Empty)
                {
                    // get Powershell help for the function with the same name as the module 
                    report = invoker.Invoke(string.Format("Get-Help {0}", scriptName));
                    return report[0].ToString();
                }

                // run the Function with the same name as the module 
                report = invoker.Invoke(string.Format("{0} {1}", scriptName, command));
                return report[0].ToString();
            }

        }


        internal static string GetPath(string appendFile)
        {
            string path = string.Empty;
            if (!(ScriptsPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) || ScriptsPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)))
            {
                path = ScriptsPath;
                path += Path.DirectorySeparatorChar;
            }
            
            return path + appendFile;
        }
    }
}
