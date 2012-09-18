using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlc.DotNet.Core;
using Words.Core;
using System.IO;
using System.Reflection;

namespace Words.AudioVideo
{
	public static class VlcController
	{
		public static bool IsAvailable
		{
			get
			{
				// TODO: returns true if 64-bit VLC is installed,
				// but this can't be used by 32-bit Words
				// TODO: make VLC path configurable
				return Directory.Exists(@"C:\Program Files\VideoLAN\VLC") || Directory.Exists(@"C:\Programme\VideoLAN\VLC");
			}
		}

		public static bool IsInitialized { get; private set; }

		public static void Init()
		{
			if (!IsAvailable)
				throw new InvalidOperationException("VLC is not available (not installed).");

			string path;
			if (Directory.Exists(@"C:\Program Files\VideoLAN\VLC"))
				path = @"C:\Program Files\VideoLAN\VLC";
			else
				path = @"C:\Programme\VideoLAN\VLC";

			//Set libvlc.dll and libvlccore.dll directory path
			//VlcContext.LibVlcDllsPath = CommonStrings.LIBVLC_DLLS_PATH_DEFAULT_VALUE_AMD64;
			//Set the vlc plugins directory path
			//VlcContext.LibVlcPluginsPath = CommonStrings.PLUGINS_PATH_DEFAULT_VALUE_AMD64;

			// TODO: what happens if VLC is not available?

			//Set the startup options
			VlcContext.StartupOptions.IgnoreConfig = true;
			//VlcContext.StartupOptions.LogOptions.LogInFile = true;
			//VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = true;
			VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.Debug;
			VlcContext.StartupOptions.AddOption("--no-osd");
			VlcContext.LibVlcDllsPath = path;
			VlcContext.LibVlcPluginsPath = path + @"\plugins";

			//Initialize the VlcContext
			try
			{
				VlcContext.Initialize();
			}
			catch (FileNotFoundException)
			{
				throw new InvalidOperationException("VLC is not available (libvlc not found). Try installing the 32-bit version of VLC.");
			}
			catch (TargetInvocationException)
			{
				throw new InvalidOperationException("VLC could not be loaded. Try installing the latest version (or any version >= 1.2).");
			}

			IsInitialized = true;
		}

		[Shutdown]
		public static void Shutdown()
		{
			if (IsInitialized)
				VlcContext.CloseAll();
		}
	}
}
