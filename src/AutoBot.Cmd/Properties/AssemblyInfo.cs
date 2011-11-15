using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AutoBot")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AutoBot")]
[assembly: AssemblyCopyright("Copyright © 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bc383ace-32a5-4ed0-adc9-575e7a203090")]

// Instructs log4net to load its configuration from the application’s configuration file.
// Setting the Watch parameter to true causes log4net to actively monitor the configuration file for changes.
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#if DEBUG
	[assembly: AssemblyInformationalVersion("1.0.0.0_TrunkDeveloperDebug")]
	[assembly: AssemblyConfiguration("Debug")]
#else
	[assembly: AssemblyInformationalVersion("1.0.0.0_TrunkDeveloperRelease")]
	[assembly: AssemblyConfiguration("Release")]
#endif
