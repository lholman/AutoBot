using System;
using System.Collections;
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

                invoker.Invoke(string.Format("Import-Module {0}", scriptPath));

                try
                {
                    // run the Function with the same name as the module 
                    IList errors;
                    psObjects = invoker.Invoke(string.Format("{0} {1}", scriptName, command), null, out errors);
                    invoker.Invoke(string.Format("Remove-Module {0}", scriptName));
                    if (errors.Count > 0)
                    {
                        string errorString = string.Empty;
                        foreach (var error in errors)
                            errorString += error.ToString();

                        ConsoleExtender.Error(errorString);
                        return new Collection<PSObject>
                                            {
                                                new PSObject(string.Format("OOohhh, I got an error running {0}.  It looks like this: {1}.", scriptName, errorString))
                                            };
                    }
                    return psObjects;

                }
                catch (Exception ex)
                {
                    ConsoleExtender.Error(ex.ToString());
                    return new Collection<PSObject>
                                            {
                                                new PSObject(string.Format("Urghhh!, that didn't taste nice!  There's a problem with me running the {0} script.  Ask your administrator for the event/error log entry.", scriptName))
                                            };
                }
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
