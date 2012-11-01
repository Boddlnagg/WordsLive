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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordsLive.Core.Songs;

namespace WordsLive.Core.Data
{
	public class LocalSongDataProvider: SongDataProvider
	{
		private string directory;

		public LocalSongDataProvider(string directory)
		{
			if (directory == null)
				throw new ArgumentNullException("directory");
			if (!Directory.Exists(directory))
				throw new ArgumentException("directory does not exist");

			this.directory = directory;
		}

		/// <summary>
		/// Gets all available songs the provider can provide.
		/// </summary>
		/// <returns>
		/// All available songs.
		/// </returns>
		public override IEnumerable<SongData> All()
		{
			SongData data;
			foreach (string file in Directory.GetFiles(directory))
			{
				data = null;

				try
				{
					data = SongData.Create(new Song(file));
				}
				catch { }

				if (data != null)
					yield return data;
			}
		}

		/// <summary>
		/// Gets the full (absolute) path for a song data object.
		/// TODO: replace this with some kind of "MediaLocator" object
		/// </summary>
		/// <param name="song">The song to get the path for.</param>
		/// <returns>
		/// The full path where the specified song can be retrieved.
		/// </returns>
		public override string GetFullPath(SongData song)
		{
			return Path.Combine(directory, song.Filename);
		}
	}
}
