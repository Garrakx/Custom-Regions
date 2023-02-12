using CustomRegions.Mod;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//[assembly: AssemblyTitle("Custom-Regions-Support")]
[assembly: AssemblyDescription("Modded regions support for Rain World")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Garrakx")]
//[assembly: AssemblyProduct("Custom-Regions-Support")]
[assembly: AssemblyCopyright("Copyright ©  2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("13837869-9236-4d05-8841-e265ab15028c")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyVersion("0.9.43.5")]
//[assembly: AssemblyFileVersion("0.9.43.5")]

[assembly: AssemblyVersion(CustomRegionsMod.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(CustomRegionsMod.PLUGIN_VERSION)]
[assembly: AssemblyTitle(CustomRegionsMod.PLUGIN_NAME + " (" + CustomRegionsMod.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(CustomRegionsMod.PLUGIN_NAME)]

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
