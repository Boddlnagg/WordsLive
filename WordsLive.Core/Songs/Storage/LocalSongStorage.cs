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

namespace WordsLive.Core.Songs.Storage
{
	public class LocalSongStorage : SongStorage
	{
		private string directory;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalSongDataProvider"/> class.
		/// </summary>
		/// <param name="directory">The songs directory.</param>
		public LocalSongStorage(string directory)
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
			// TODO: implement caching
			SongData data;
			foreach (string file in Directory.GetFiles(directory))
			{
				data = null;

				try
				{
					var song = new Song(new Uri("song:///" + Path.GetFileName(file)), new SongUriResolver(this));
					data = SongData.Create(song);
				}
				catch { }

				if (data != null)
					yield return data;
			}
		}

		/// <summary>
		/// Gets the resource at the specified path.
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		/// <returns>
		/// The resource as a stream.
		/// </returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		public override Stream Get(string name)
		{
			if (name.Contains("\\") || name.Contains("/"))
				throw new ArgumentException("Song filename must not contain a full path.");

			string fullPath = Path.Combine(directory, name);

			if (!File.Exists(fullPath))
				throw new FileNotFoundException(name);

			return File.OpenRead(fullPath);
		}

		public override FileTransaction Put(string path)
		{
			return new LocalFileTransaction(Path.Combine(directory, path));
		}

		public override void Delete(string name)
		{
			File.Delete(Path.Combine(directory, name));
		}

		public override FileInfo GetLocal(string name)
		{
			var fi = new FileInfo(Path.Combine(directory, name));

			if (!fi.Exists)
				throw new FileNotFoundException(name);

			return fi;
		}

		public override bool Exists(string name)
		{
			return File.Exists(Path.Combine(directory, name));
		}

		public override Uri TryRewriteUri(Uri uri)
		{
			if (uri.IsFile)
			{
				// rewrite local song URIs to song:// schema
				var store = DataManager.Songs as LocalSongStorage;
				try
				{
					FileInfo fi1 = new FileInfo(Uri.UnescapeDataString(uri.AbsolutePath));
					var name = Uri.UnescapeDataString(uri.Segments[uri.Segments.Length - 1]);
					var fi2 = store.GetLocal(name);
					if (fi1.Exists && fi2.Exists && fi1.FullName == fi2.FullName)
					{
						return new Uri("song:///" + name);
					}
				}
				catch (FileNotFoundException)
				{
					return uri;
				}
			}

			return uri;
		}
	}
}
