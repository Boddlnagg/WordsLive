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
using WordsLive.Core.Songs.Storage;

namespace WordsLive.Core.Data
{
	public static class DataManager
	{
		private static FileInfo songTemplate;
		private static TemporaryDirectory tempDirectory;

		// TODO: move into Storage namespace
		public static SongStorage ActualSongStorage { get; private set; }
		public static BackgroundStorage ActualBackgroundStorage { get; private set; }

		private static SongStorage redirectSongStorage;
		private static BackgroundStorage redirectBackgroundStorage;

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
		/// Gets the storage for songs.
		/// </summary>
		public static SongStorage Songs
		{
			get
			{
				if (redirectSongStorage != null)
					return redirectSongStorage;
				else
					return ActualSongStorage;
			}
		}

		/// <summary>
		/// Gets the storage for backgrounds.
		/// </summary>
		public static BackgroundStorage Backgrounds
		{
			get
			{
				if (redirectBackgroundStorage != null)
					return redirectBackgroundStorage;
				else
					return ActualBackgroundStorage;
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

			var songs = new HttpSongStorage(address + "/songs/", credentials);
			var backgrounds = new HttpBackgroundStorage(address + "/backgrounds/", credentials);

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

			ActualSongStorage = songs;
			ActualBackgroundStorage = backgrounds;
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
			var songs = new LocalSongStorage(songsDirectory);

			if (backgroundsDirectory.EndsWith("\\"))
				backgroundsDirectory = backgroundsDirectory.Substring(0, backgroundsDirectory.Length - 1);
			var backgrounds = new LocalBackgroundStorage(backgroundsDirectory)
			{
				// TODO: add more?
				AllowedImageExtensions = new string[] { ".png", ".jpg", ".jpeg" },
				AllowedVideoExtensions = new string[] { ".mp4", ".wmv", ".avi", ".ogv" }
			};

			ActualSongStorage = songs;
			ActualBackgroundStorage = backgrounds;
			return true;
		}

		/// <summary>
		/// Enables redirecting of all request over local server using the given password.
		/// </summary>
		/// <param name="password">The password.</param>
		public static void EnableRedirect(SongStorage songs, BackgroundStorage backgrounds)
		{
			redirectSongStorage = songs;
			redirectBackgroundStorage = backgrounds;
		}

		/// <summary>
		/// Disables the redirect.
		/// </summary>
		public static void DisableRedirect()
		{
			redirectSongStorage = null;
			redirectBackgroundStorage = null;
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
