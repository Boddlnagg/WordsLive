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

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Represents a directory that contains backgrounds. An instance of this class can only be obtained
	/// using an instance of <see cref="BackgroundStorage"/>.
	/// </summary>
	public class BackgroundDirectory
	{
		private BackgroundStorage storage;

		/// <summary>
		/// The path of the parent (with leading and trailing '/') or <c>null</c> if it's the root.
		/// </summary>
		private string parentPath;

		internal BackgroundDirectory(BackgroundStorage storage, string path)
		{
			this.storage = storage;
			int i = path.Substring(0, path.Length - 1).LastIndexOf('/');
			if (i < 0)
			{
				this.parentPath = null;
				this.Name = String.Empty;
			}
			else
			{
				this.parentPath = path.Substring(0, i + 1);
				this.Name = path.Substring(i + 1, path.Length - (i + 2));
			}
		}

		/// <summary>
		/// Gets the name of this directory.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the parent directory
		/// </summary>
		public BackgroundDirectory Parent
		{
			get
			{
				if (parentPath == null)
					return null;
				else
					return new BackgroundDirectory(this.storage, this.parentPath);
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether this is the root directory.
		/// </summary>
		public bool IsRoot
		{
			get
			{
				return Parent == null;
			}
		}

		/// <summary>
		/// Gets the path of this directory (relative to the root)
		/// using '/' as a separator and with a leading and trailing '/'.
		/// </summary>
		public string Path
		{
			get
			{
				return parentPath + Name + "/";
			}
		}

		/// <summary>
		/// Gets the subdirectories in this directory.
		/// </summary>
		public IEnumerable<BackgroundDirectory> Directories
		{
			get
			{
				return storage.GetDirectories(this);
			}
		}

		/// <summary>
		/// Gets the actual backgrounds in this directory.
		/// </summary>
		public IEnumerable<BackgroundFile> Files
		{
			get
			{
				return storage.GetFiles(this);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as BackgroundDirectory;

			if (other == null)
				return false;
			else
				return other.Path == this.Path && other.storage == this.storage;
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode() ^ storage.GetHashCode();
		}
	}
}
