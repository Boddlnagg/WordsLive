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

namespace WordsLive.Core.Data
{
	public class BackgroundFile
	{
		private BackgroundDataProvider provider;

		/// <summary>
		/// Gets the name of this file.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this background is a video.
		/// </summary>
		public bool IsVideo { get; private set; }

		/// <summary>
		/// Gets the parent directory.
		/// </summary>
		public BackgroundDirectory Parent { get; private set; }

		internal BackgroundFile(BackgroundDataProvider provider, BackgroundDirectory parent, string name, bool isVideo)
		{
			this.provider = provider;
			this.Name = name;
			this.Parent = parent;
			this.IsVideo = isVideo;
		}

		/// <summary>
		/// Gets the path of this file (relative to the root)
		/// using '/' as a separator and with a leading '/'.
		/// </summary>
		public string Path
		{
			get
			{
				return Parent.Path + Name;
			}
		}

		/// <summary>
		/// Gets the URI of this file.
		/// </summary>
		public Uri Uri
		{
			get
			{
				return provider.GetFileUri(this);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as BackgroundFile;

			if (other == null)
				return false;
			else
				return other.Path == this.Path && other.provider == this.provider;
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode() ^ provider.GetHashCode();
		}
	}
}
