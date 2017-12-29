using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Artech.Architecture.Common.Packages;


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("KBDoctor")]
[assembly: AssemblyDescription("KBDoctor its a tool to diagnose common problems in Knowledge Bases.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Concepto")]
[assembly: AssemblyProduct("KBDoctor")]
[assembly: AssemblyCopyright("Copyright 2007-2016")]
[assembly: AssemblyTrademark("Concepto")]
[assembly: AssemblyCulture("")]

// The following attributes are declarations related to this assembly
// as a GeneXus Package
  [assembly: PackageAttribute(typeof(Concepto.Packages.KBDoctor.Package), IsCore = false, IsUIPackage = true )] 

//[assembly: PackageAttribute(typeof(Artech.Packages.TeamDevClient.Package), IsCore = false, IsUIPackage = false)]
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7d9bfa9c-9b9c-4b03-bd6d-35c3edd01b57")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("10.16.*")]
[assembly: AssemblyFileVersion("10.16.0")]

//[assembly: PackageAttribute(typeof(Artech.Packages.TeamDevClient.Package), IsCore = false, IsUIPackage = false)]

