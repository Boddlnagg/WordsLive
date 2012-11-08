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

namespace WordsLive.Core.Data
{
	public static class DataManager
	{
		/// <summary>
		/// Initializes the <see cref="DataManager"/> class.
		/// </summary>
		static DataManager()
		{
			LocalFiles = new LocalFileDataProvider();
		}

		/// <summary>
		/// Initializes the directories for songs and backgrounds.
		/// </summary>
		/// <param name="songsDirectory">The directory for songs.</param>
		/// <param name="backgroundsDirectory">The directory for backgrounds.</param>
		public static void InitLocalDirectories(string songsDirectory, string backgroundsDirectory)
		{
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

			System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("WordsLive", "abc");
			Songs = new HttpSongDataProvider("http://wordslive.media/songs/", credentials);
			Backgrounds = new HttpBackgroundDataProvider("http://wordslive.media/backgrounds/", credentials);
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
	}
}
