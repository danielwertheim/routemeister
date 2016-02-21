using System.Runtime.InteropServices;
 using System.Reflection;

#if DEBUG
[assembly: AssemblyProduct("Routemeister (Debug)")]
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyProduct("Routemeister (Release)")]
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyDescription("Routemeister - for in-process async message routing.")]
[assembly: AssemblyCompany("Daniel Wertheim")]
[assembly: AssemblyCopyright("Copyright © 2016 Daniel Wertheim")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.0.0.*")]
[assembly: AssemblyInformationalVersion("0.0.0.*")]