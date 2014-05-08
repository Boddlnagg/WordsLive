# WordsLive #
## worship projection software ##

For more information, see [wordslive.org](http://wordslive.org).

If you want to contribute, feel free to ...

- ... open a new issue here on GitHub to report a bug or request a new feature
- ... fork the repository and create pull requests
- ... help creating the documentation ([User Manual](http://wordslive.org/manual))

This project is licensed under the terms of the GPLv3.

### How to build
The following tools are required for building WordsLive:

- Visual Studio 2012 or later
- [WiX Toolset](http://wixtoolset.org/) (for building the installer)
- Office PowerPoint and LibreOffice (for building the bridge assemblies)

On first build, NuGet will download some assemblies required for async support on .NET 4 (this is required in order to support Windows XP, but will probably be dropped in the future). After this, you need to restart Visual Studio to complete the build process successfully!
