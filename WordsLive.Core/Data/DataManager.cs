/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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

namespace WordsLive.Core.Data
{
	public static class DataManager
	{
		private static FileInfo songTemplate;
		private static TemporaryDirectory tempDirectory;

		public static SongDataProvider ActualSongDataProvider { get; private set; }
		public static BackgroundDataProvider ActualBackgroundDataProvider { get; private set; }

		private static SongDataProvider redirectSongDataProvider;
		private static BackgroundDataProvider redirectBackgroundDataProvider;

		public static DirectoryInfo TempDirectory
		{
			get
			{
				if (tempDirectory == null)
					tempDirectory = new TemporaryDirectory();

				return tempDirectory.Directory;
			}
		}

		/// <summary>
		/// Gets the data provider for songs.
		/// </summary>
		public static SongDataProvider Songs
		{
			get
			{
				if (redirectSongDataProvider != null)
					return redirectSongDataProvider;
				else
					return ActualSongDataProvider;
			}
		}

		/// <summary>
		/// Gets the data provider for backgrounds.
		/// </summary>
		public static BackgroundDataProvider Backgrounds
		{
			get
			{
				if (redirectBackgroundDataProvider != null)
					return redirectBackgroundDataProvider;
				else
					return ActualBackgroundDataProvider;
			}
		}

		/// <summary>
		/// Gets the data provider for local files.
		/// </summary>
		public static LocalFileDataProvider LocalFiles { get; private set; }

		/// <summary>
		/// Gets or sets the file used as template for songs.
		/// </summary>
		public static FileInfo SongTemplate
		{
			get
			{
				return songTemplate;
			}
			set
			{
				if (!value.Exists)
					throw new FileNotFoundException(value.FullName);

				songTemplate = value;
			}
		}

		/// <summary>
		/// Initializes the <see cref="DataManager"/> class.
		/// </summary>
		static DataManager()
		{
			LocalFiles = new LocalFileDataProvider();
		}

		/// <summary>
		/// Tries the initialize this data manager using a data server for songs and backgrounds.
		/// </summary>
		/// <param name="address">The address of the server.</param>
		/// <param name="password">The password to use, <c>null</c> or an empty string if no authentication should be used.</param>
		/// <returns><c>true</c> if the initialization succeeded, <c>false</c> otherwise.</returns>
		public static bool TryInitUsingServer(string address, string password)
		{
			if (address.EndsWith("/"))
				address = address.Substring(0, address.Length - 1);

			if (String.IsNullOrWhiteSpace(address))
				return false;

			System.Net.NetworkCredential credentials = null;

			if (!String.IsNullOrWhiteSpace(password))
				credentials = new System.Net.NetworkCredential("WordsLive", password);

			var songs = new HttpSongDataProvider(address + "/songs/", credentials);
			var backgrounds = new HttpBackgroundDataProvider(address + "/backgrounds/", credentials);

			try
			{
				songs.Count(); // get song count for testing the connection and password
			}
			catch (System.Net.WebException)
			{
				return false; // problem with the connection
			}
			catch (FormatException)
			{
				return false; // problem with the response (not a number)
			}

			ActualSongDataProvider = songs;
			ActualBackgroundDataProvider = backgrounds;
			return true;
		}

		/// <summary>
		/// Tries to initialize this data manager using local directories for songs and backgrounds.
		/// </summary>
		/// <param name="songsDirectory">The directory for songs.</param>
		/// <param name="backgroundsDirectory">The directory for backgrounds.</param>
		/// <returns><c>true</c> if the initialization succeeded, <c>false</c> otherwise.</returns>
		public static bool TryInitUsingLocal(string songsDirectory, string backgroundsDirectory)
		{
			if (!Directory.Exists(songsDirectory) || !Directory.Exists(backgroundsDirectory))
				return false;

			if (songsDirectory.EndsWith("\\"))
				songsDirectory = songsDirectory.Substring(0, songsDirectory.Length - 1);
			var songs = new LocalSongDataProvider(songsDirectory);

			if (backgroundsDirectory.EndsWith("\\"))
				backgroundsDirectory = backgroundsDirectory.Substring(0, backgroundsDirectory.Length - 1);
			var backgrounds = new LocalBackgroundDataProvider(backgroundsDirectory)
			{
				// TODO: add more?
				AllowedImageExtensions = new string[] { ".png", ".jpg", ".jpeg" },
				AllowedVideoExtensions = new string[] { ".mp4", ".wmv", ".avi", ".ogv" }
			};

			ActualSongDataProvider = songs;
			ActualBackgroundDataProvider = backgrounds;
			return true;
		}

		/// <summary>
		/// Enables redirecting of all request over local server using the given password.
		/// </summary>
		/// <param name="password">The password.</param>
		public static void EnableRedirect(SongDataProvider songs, BackgroundDataProvider backgrounds)
		{
			redirectSongDataProvider = songs;
			redirectBackgroundDataProvider = backgrounds;
		}

		/// <summary>
		/// Disables the redirect.
		/// </summary>
		public static void DisableRedirect()
		{
			redirectSongDataProvider = null;
			redirectBackgroundDataProvider = null;
		}

		public class TemporaryDirectory
		{
			public DirectoryInfo Directory { get; set; }

			public TemporaryDirectory()
			{
				DirectoryInfo tmpdir;
				do
				{
					tmpdir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WordsLive_" + Path.GetRandomFileName()));
				} while (tmpdir.Exists);

				tmpdir.Create();
				Directory = tmpdir;
			}

			private void DeleteDirectoryRecursive(DirectoryInfo directory)
			{
				try
				{
					foreach (var file in directory.GetFiles())
					{
						file.Delete();
					}

					foreach (var dir in directory.GetDirectories())
					{
						DeleteDirectoryRecursive(dir);
					}

					directory.Delete();
				}

				catch { }
			}

			~TemporaryDirectory()
			{
				DeleteDirectoryRecursive(Directory);
			}
		}
	}
}
