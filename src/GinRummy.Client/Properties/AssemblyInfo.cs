using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyTitle("GinRummy.Client")]
[assembly: AssemblyDescription("Gin Rummy 2D client. Internationalized graphical user interface.")]
[assembly: AssemblyCompany("Equipo 6")]
[assembly: AssemblyProduct("Gin Rummy 2D")]
[assembly: ComVisible(false)]

// The base culture of the project is es-MX, so Strings.resx holds the Spanish text
// and ships inside the main assembly. Strings.en-US.resx builds a satellite assembly.
[assembly: NeutralResourcesLanguage("es-MX")]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: AssemblyVersion("0.2.0.0")]
[assembly: AssemblyFileVersion("0.2.0.0")]
