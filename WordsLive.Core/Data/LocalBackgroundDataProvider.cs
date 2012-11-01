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
				return new BackgroundDirectory(this, null, Path.GetFileName(directory));
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
		/// Gets all available background files in a specified directory.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <returns>
		/// A list of background filenames (relative to the specified directory).
		/// </returns>
		public override IEnumerable<BackgroundFile> GetFiles(BackgroundDirectory dir)
		{
			if (dir == null)
				throw new ArgumentNullException("dir");

			DirectoryInfo info = new DirectoryInfo(GetPath(dir));

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

			DirectoryInfo info = new DirectoryInfo(GetPath(parent));

			if (!info.Exists)
				throw new ArgumentException("Directory does not exist");

			foreach (var dir in info.GetDirectories().Where(d => d.Name != "[Thumbnails]"))
			{
				yield return new BackgroundDirectory(this, parent, dir.Name);
			}
		}

		/// <summary>
		/// Helper function to get path string for a given directory by recursivly walking up the directory structure.
		/// </summary>
		/// <param name="dir">The directory to get the path for.</param>
		/// <returns>The path.</returns>
		private string GetPath(BackgroundDirectory dir)
		{
			if (dir.IsRoot)
				return directory;

			return Path.Combine(GetPath(dir.Parent), dir.Name);
		}
	}
}
