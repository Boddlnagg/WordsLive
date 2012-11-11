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
using WordsLive.Core.Songs;

namespace WordsLive.Core.Data
{
	public static class DataManager
	{
		private static FileInfo songTemplate;

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
				address.Substring(0, address.Length - 1);

			if (String.IsNullOrWhiteSpace(address))
				return false;

			System.Net.NetworkCredential credentials = null;

			if (!String.IsNullOrWhiteSpace(password))
				credentials = new System.Net.NetworkCredential("WordsLive", password);

			Songs = new HttpSongDataProvider(address + "/songs/", credentials);
			Backgrounds = new HttpBackgroundDataProvider(address + "/backgrounds/", credentials);

			try
			{
				Songs.Count(); // get song count for testing the connection and password
			}
			catch (System.Net.WebException)
			{
				return false; // problem with the connection
			}
			catch (FormatException)
			{
				return false; // problem with the response (not a number)
			}

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
			Songs = new LocalSongDataProvider(songsDirectory);

			if (backgroundsDirectory.EndsWith("\\"))
				backgroundsDirectory = backgroundsDirectory.Substring(0, backgroundsDirectory.Length - 1);
			Backgrounds = new LocalBackgroundDataProvider(backgroundsDirectory)
			{
				// TODO: add more?
				AllowedImageExtensions = new string[] { ".png", ".jpg", ".jpeg" },
				AllowedVideoExtensions = new string[] { ".mp4", ".wmv", ".avi" }
			};

			return true;
		}

		/// <summary>
		/// Gets the data provider for songs.
		/// </summary>
		public static SongDataProvider Songs { get; private set; }

		/// <summary>
		/// Gets the data provider for backgrounds.
		/// </summary>
		public static BackgroundDataProvider Backgrounds { get; private set; }

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
	}
}
