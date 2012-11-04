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

namespace WordsLive.Core.Data
{
	public class LocalBackgroundDataProvider : BackgroundDataProvider
	{
		private string directory;

		/// <summary>
		/// Gets or sets the allowed image extensions.
		/// </summary>
		public IEnumerable<string> AllowedImageExtensions { get; set; }

		/// <summary>
		/// Gets or sets the allowed video extensions.
		/// </summary>
		public IEnumerable<string> AllowedVideoExtensions { get; set; }

		/// <summary>
		/// Gets the root directory.
		/// </summary>
		public override BackgroundDirectory Root
		{
			get
			{
				return new BackgroundDirectory(this, "/");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalBackgroundDataProvider"/> class.
		/// </summary>
		/// <param name="directory">The root directory.</param>
		public LocalBackgroundDataProvider(string directory)
		{
			if (directory == null)
				throw new ArgumentNullException("directory");

			if (!Directory.Exists(directory))
				throw new ArgumentException("directory does not exist");

			this.directory = directory;
		}

		/// <summary>
		/// Gets the file specified by a path. The path has to start with '/' and use
		/// '/' as directory separator.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The background file instance.</returns>
		/// <exception cref="FileNotFoundException">The file was not found.</exception>
		public override BackgroundFile GetFile(string path)
		{
			var file = new FileInfo(Path.Combine(directory, path.Substring(1).Replace('/', '\\')));

			if (!file.Exists)
				throw new FileNotFoundException(path + " not found.");

			string dir = path.Substring(0, path.LastIndexOf('/') + 1);

			if (AllowedImageExtensions != null && AllowedImageExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
			{
				return new BackgroundFile(this, new BackgroundDirectory(this, dir), file.Name, false);
			}
			else if (AllowedVideoExtensions != null && AllowedVideoExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
			{
				return new BackgroundFile(this, new BackgroundDirectory(this, dir), file.Name, true);
			}
			else
			{
				throw new FileNotFoundException(path + " not found.");
			}
		}

		/// <summary>
		/// Gets all available background files in a specified directory.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <returns>
		/// A list of background files (relative to the specified directory).
		/// </returns>
		public override IEnumerable<BackgroundFile> GetFiles(BackgroundDirectory dir)
		{
			if (dir == null)
				throw new ArgumentNullException("dir");

			DirectoryInfo info = GetAbsoluteDirectory(dir);

			if (!info.Exists)
				throw new ArgumentException("Directory does not exist");

			foreach (var file in info.GetFiles())
			{
				if (AllowedImageExtensions != null && AllowedImageExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
				{
					yield return new BackgroundFile(this, dir, file.Name, false);
				}
				else if (AllowedVideoExtensions != null && AllowedVideoExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
				{
					yield return new BackgroundFile(this, dir, file.Name, true);
				}
			}
		}

		/// <summary>
		/// Gets all subdirectories of a specified directory.
		/// </summary>
		/// <param name="parent">The parent directory.</param>
		/// <returns>
		/// A list of subdirectories.
		/// </returns>
		public override IEnumerable<BackgroundDirectory> GetDirectories(BackgroundDirectory parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			DirectoryInfo info = GetAbsoluteDirectory(parent);

			if (!info.Exists)
				throw new ArgumentException("Directory does not exist");

			foreach (var dir in info.GetDirectories().Where(d => d.Name != "[Thumbnails]"))
			{
				yield return new BackgroundDirectory(this, parent.Path + dir.Name + "/");
			}
		}

		/// <summary>
		/// Gets the URI of a background file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>
		/// The file's URI.
		/// </returns>
		public override Uri GetFileUri(BackgroundFile file)
		{
			var realPath = Path.Combine(directory, file.Path.Substring(1).Replace('/', Path.DirectorySeparatorChar));
			return new Uri(realPath);
		}

		/// <summary>
		/// Helper function to get the absolute path for a given directory.
		/// </summary>
		/// <param name="dir">The directory to get the path for.</param>
		/// <returns>A DirectoryInfo for the absolute path.</returns>
		private DirectoryInfo GetAbsoluteDirectory(BackgroundDirectory dir)
		{
			if (dir.IsRoot)
				return new DirectoryInfo(directory);

			return new DirectoryInfo(directory + dir.Path.Replace('/', Path.DirectorySeparatorChar));
		}
	}
}
