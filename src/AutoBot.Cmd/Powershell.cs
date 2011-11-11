using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace AutoBot.Cmd
{
    public class Powershell
    {
        private static readonly string ScriptsPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Scripts");

        internal static Collection<PSObject> RunPowershellModule(string scriptName, string command)
        {

            if (!File.Exists(GetPath(scriptName)))
            {
                return new Collection<PSObject>
                                            {
                                                new PSObject("Unknown command!, try Get-Help instead")
                                            };
            }
            using (RunspaceInvoke invoker = new RunspaceInvoke())
            {
                string scriptPath = GetPath(scriptName);
                Collection<PSObject> psObjects;

                if (scriptName == "Get-Help" && command == string.Empty)
                    invoker.Invoke(string.Format("Import-Module {0}", scriptPath));

                if (scriptName != "Get-Help")
                    invoker.Invoke(string.Format("Import-Module {0}", scriptPath));

                // run the Function with the same name as the module 
                psObjects = invoker.Invoke(string.Format("{0} {1}", scriptName, command));
                return psObjects;
            }

        }


        internal static string GetPath(string filenameWithoutExtension)
        {
            string path = string.Empty;
            if (!(ScriptsPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) || ScriptsPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)))
            {
                path = ScriptsPath;
                path += Path.DirectorySeparatorChar;
            }
            
            return path + filenameWithoutExtension + ".psm1";
        }
    }
}
