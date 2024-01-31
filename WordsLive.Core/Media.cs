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
using System.Linq;

namespace WordsLive.Core
{
	/// <summary>
	/// Abstract base class for all media types.
	/// </summary>
	public abstract class Media
	{
		/// <summary>
		/// Gets the URI associated with this media object.
		/// <c>null</c> if this media object has not been loaded from a URI and has not been saved yet.
		/// </summary>
		public Uri Uri { get; protected set; }


		private Dictionary<string, string> options;

		public event EventHandler OptionsChanged;

		protected void OnOptionsChanged()
		{
			if (OptionsChanged != null)
				OptionsChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Gets a dictionary (key-value-store) with additional options.
		/// </summary>
		public Dictionary<string, string> Options
		{
			get
			{
				if (options == null)
					options = new Dictionary<string, string>();

				return options;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Media"/> class.
		/// </summary>
		/// <param name="uri">The URI pointing to the media resource.</param>
		public Media(Uri uri)
		{
			Uri = uri;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Media"/> class.
		/// </summary>
		/// <param name="uri">The URI pointing to the media resource.</param>7
		/// <param name="options">Additional options.</param>
		public Media(Uri uri, Dictionary<string, string> options)
		{
			Uri = uri;
			this.options = options;
		}

		/// <summary>
		/// Loads just the metadata of this media object.
		/// </summary>
		protected virtual void LoadMetadata()
		{
			// do nothing
		}

		/// <summary>
		/// Internal interface to LoadMetadata(). This is called in  <see cref="MediaManager.ReloadMediaMetadata"/>.
		/// </summary>
		internal void LoadMetadataHelper()
		{
			LoadMetadata();
		}

		/// <summary>
		/// Gets the title of this media object. The basic implementation just returns the file name.
		/// If you want to set a custom title, override this method.
		/// </summary>
		public virtual string Title
		{
			get
			{
				if (Uri == null)
				{
					return null;
				}
				else
				{
					return Uri.UnescapeDataString(Uri.Segments.Last());
				}
			}
		}

		/// <summary>
		/// Loads the media object from the file specified in the <see cref="File"/> field into memory.
		/// This is always called before the control panel and/or presentation is shown.
		/// Use <see cref="MediaManager.LoadMedia"/> to call this safely.
		/// </summary>
		public abstract void Load();
	}
}
