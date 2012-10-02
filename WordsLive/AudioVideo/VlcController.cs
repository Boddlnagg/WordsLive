using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlc.DotNet.Core;
using WordsLive.Core;
using System.IO;
using System.Reflection;

namespace WordsLive.AudioVideo
{
	public static class VlcController
	{
		public static string Path
		{
			get
			{
				// TODO: make VLC path configurable
				string directory;
				if (Environment.Is64BitOperatingSystem)
				{
					directory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
				}
				else
				{
					directory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				}

				return directory + @"\VideoLAN\VLC";
			}
		}

		public static bool IsAvailable
		{
			get
			{
				return Directory.Exists(Path);
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
			VlcContext.LibVlcDllsPath = Path;
			VlcContext.LibVlcPluginsPath = Path + @"\plugins";

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
