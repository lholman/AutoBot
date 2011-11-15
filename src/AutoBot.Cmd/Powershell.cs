using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using log4net;

namespace AutoBot.Cmd
{
    public class Powershell
    {
        private static readonly string ScriptsPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Scripts");
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        internal static Collection<PSObject> RunPowershellModule(string scriptName, string command)
        {

            if (!File.Exists(GetPath(scriptName)))
            {
                return new Collection<PSObject>
                                            {
                                                new PSObject("Unknown command!, try \"@autobot Get-Help\" instead")
                                            };
            }
            using (RunspaceInvoke invoker = new RunspaceInvoke())
            {
                string scriptPath = GetPath(scriptName);
                Collection<PSObject> psObjects;

                invoker.Invoke(string.Format("Import-Module {0}", scriptPath));

                try
                {
                    // execute the PowerShell Function with the same name as the module 
                    IList errors;
                    psObjects = invoker.Invoke(string.Format("{0} {1}", scriptName, command), null, out errors);
                    invoker.Invoke(string.Format("Remove-Module {0}", scriptName));
                    if (errors.Count > 0)
                    {
                        string errorString = string.Empty;
                        foreach (var error in errors)
                            errorString += error.ToString();

                        _logger.Error(string.Format("ERROR!: {0}", errorString));
                        return new Collection<PSObject>
                                            {
                                                new PSObject(string.Format("OOohhh, I got an error running {0}.  It looks like this: \r\n{1}.", scriptName, errorString))
                                            };
                    }
                    return psObjects;

                }
                catch (Exception ex)
                {
                    _logger.Error("ERROR!:", ex);
                    string errorText = string.Format("Urghhh!, that didn't taste nice!  There's a problem with me running the {0} script. \r\n", scriptName);
                    errorText += String.Format("Check you are calling the script correctly by using \"@autobot get-help {0}\" \r\n", scriptName);
                    errorText += "If all else fails ask your administrator for the event/error log entry.";
                    
                    return new Collection<PSObject>
                                            {
                                                new PSObject(errorText)
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
