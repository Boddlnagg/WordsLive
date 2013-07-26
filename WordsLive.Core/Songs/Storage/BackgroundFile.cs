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

namespace WordsLive.Core.Songs.Storage
{
	public class BackgroundFile
	{
		private BackgroundStorage storage;

		/// <summary>
		/// Gets the name of this file.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this background is a video.
		/// </summary>
		public bool IsVideo
		{
			get
			{
				var ext = Extension;

				if (ext == String.Empty)
					return false;
				else
					return storage.AllowedVideoTypes.ContainsKey(Extension);
			}
		}

		/// <summary>
		/// Gets the parent directory.
		/// </summary>
		public BackgroundDirectory Parent { get; private set; }

		internal BackgroundFile(BackgroundStorage storage, BackgroundDirectory parent, string name)
		{
			this.storage = storage;
			this.Name = name;
			this.Parent = parent;
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
				return storage.GetFileUri(this);
			}
		}

		/// <summary>
		/// Gets the URI of a preview of this file.
		/// If none is available, it will be the same as <see cref="Uri"/>.
		/// </summary>
		public Uri PreviewUri
		{
			get
			{
				return storage.GetPreviewUri(this);
			}
		}

		public string Extension
		{
			get
			{
				var di = Name.LastIndexOf('.');

				if (di == -1)
					return String.Empty;

				return Name.Substring(di).ToLowerInvariant();
			}
		}

		public string MimeType
		{
			get
			{
				var ext = Extension;

				if (String.IsNullOrEmpty(ext))
					return String.Empty;

				if (storage.AllowedImageTypes.ContainsKey(ext))
					return storage.AllowedImageTypes[ext];

				if (storage.AllowedVideoTypes.ContainsKey(ext))
					return storage.AllowedVideoTypes[ext];

				return String.Empty;
			}
		}

		public bool Exists
		{
			get
			{
				return storage.FileExists(this);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as BackgroundFile;

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
