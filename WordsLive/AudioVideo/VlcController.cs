/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Reflection;
using Vlc.DotNet.Core;
using WordsLive.Core;

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
