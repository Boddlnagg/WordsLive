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

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalSongDataProvider"/> class.
		/// </summary>
		/// <param name="directory">The songs directory.</param>
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
					data = SongData.Create(new Song(file, this));
				}
				catch { }

				if (data != null)
					yield return data;
			}
		}

		/// <summary>
		/// Gets the resource at the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>
		/// The resource as a stream.
		/// </returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		public override Stream Get(string path)
		{
			if (path.Contains('\\') || path.Contains('/'))
				throw new ArgumentException("Song filename must not contain a full path.");

			string fullPath = Path.Combine(directory, path);

			if (!File.Exists(fullPath))
				throw new FileNotFoundException(path);

			return File.OpenRead(fullPath);
		}

		public override Uri GetUri(string path)
		{
			return new Uri(Path.Combine(directory, path));
		}

		/// <summary>
		/// Gets the resource as a local file. If it actually is not a local file,
		/// it is temporarily cached locally.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>
		/// The resource as a local file.
		/// </returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		public override FileInfo GetLocal(string path)
		{
			var fi = new FileInfo(Path.Combine(directory, path));

			if (!fi.Exists)
				throw new FileNotFoundException(path);

			return fi;
		}
	}
}
