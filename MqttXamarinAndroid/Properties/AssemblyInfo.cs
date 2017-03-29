using System.Reflection;
using System.Runtime.CompilerServices;
using Android.App;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.
#if DEBUG
[assembly: Application(Debuggable = true,Icon ="@mipmap/ic_launcher")]

#else
[assembly: Application(Debuggable = false,Icon ="@mipmap/ic_launcher")]
#endif
[assembly: AssemblyTitle ("SmartLoadControl")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("")]
[assembly: AssemblyCopyright ("mqstarx")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]


// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

//[assembly: AssemblyVersion ("1.0.0")]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

