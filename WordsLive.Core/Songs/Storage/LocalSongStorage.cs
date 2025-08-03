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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

			foreach (string file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
			{
				data = null;

				try
				{
					var relativePath = new Uri(directory + Path.DirectorySeparatorChar)
						.MakeRelativeUri(new Uri(file))
						.ToString();
					var song = new Song(new Uri("song:///" + relativePath), new SongUriResolver(this));
					data = SongData.Create(song);
				}
				catch { }

				if (data != null)
					yield return data;
			}
		}

		public override Task<IEnumerable<SongData>> AllAsync()
		{
			return TaskHelpers.FromResult(All());
		}

		/// <summary>
		/// Gets the resource at the specified path.
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		/// <returns>
		/// The resource as a stream.
		/// </returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		public override SongStorageEntry Get(string name)
		{
			if (Path.IsPathRooted(name))
				throw new ArgumentException("Song filename must not contain a full path.");

			string fullPath = Path.Combine(directory, name);

			if (!File.Exists(fullPath))
				throw new FileNotFoundException(name);

			return new SongStorageEntry(File.OpenRead(fullPath), File.GetLastWriteTimeUtc(fullPath));
		}

		public override Task<SongStorageEntry> GetAsync(string name, CancellationToken cancellation = default(CancellationToken))
		{
			return TaskHelpers.FromResult(Get(name));
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
				var filePath = Path.GetFullPath(Uri.UnescapeDataString(uri.LocalPath));
				var rootPath = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;

				if (filePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
				{
					var relativePath = filePath.Substring(rootPath.Length).Replace(Path.DirectorySeparatorChar, '/');
					return new Uri("song:///" + relativePath);
				}
			}

			return uri;
		}
	}
}
