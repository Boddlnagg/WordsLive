using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlc.DotNet.Core;
using Words.Core;
using System.IO;

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
				return Directory.Exists(VlcContext.LibVlcDllsPath); 
			}
		}

		public static bool IsInitialized { get; private set; }

		public static void Init()
		{
			if (!IsAvailable)
				throw new InvalidOperationException("VLC is not available (not installed).");

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
			VlcContext.LibVlcDllsPath = @"C:\Program Files\VideoLAN\VLC";
			VlcContext.LibVlcPluginsPath = @"C:\Program Files\VideoLAN\VLC\plugins";

			//Initialize the VlcContext
			try
			{
				VlcContext.Initialize();
			}
			catch (FileNotFoundException)
			{
				throw new InvalidOperationException("VLC is not available (libvlc not found). Try installing the 32-bit version of VLC.");
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
