using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin. Generate a fresh one for your plugin!
[assembly: Guid("2b8caa03-b7c2-47e2-aa54-49190f7a0ea8")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.1.0.1")]
[assembly: AssemblyFileVersion("1.1.0.1")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("AstroColibri")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Integration of AstroColibri Events")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Christoph Nieswand")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("AstroColibri")]
[assembly: AssemblyCopyright("Copyright © 2025 Christoph Nieswand")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.2017")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/chrisastrophoto/Nina.AstroColibri")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage URL - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "AstroColibri, MultiMessenger, Transients")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/chrisastrophoto/Nina.AstroColibri/blob/master/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "https://astro-colibri.science/static/Pictures/deco/COLIBRI_Logo400.png")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"# Astro-Colibri Plugin

This is a plugin which
- informs the user on new events in Astro-Colibri (if visible from the current location)
- shows altitude charts of all events in the current NINA session on a specific pane on the imaging tab
- provides a button to send the object to the framing assistant

The plugin is activated by adding the AstroColibri trigger to a sequence.
The trigger condition is evaluated after each frame and as long as the sequence is running.

See more about [Astro-Colibris API here](https://astro-colibri.science/apidoc)

For more information please see [the readme](https://github.com/chrisastrophoto/Nina.AstroColibri/blob/master/README.md)

# Suggestion Welcome

I'm open to suggestions, please submit one via [GitHub's issue tracker](https://github.com/chrisastrophoto/Nina.AstroColibri/issues).

# Contact

# 3rd Party Licences

- [Astro-Colibri.Science](https://astro-colibri.science/tos)")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]