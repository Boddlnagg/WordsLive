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
	/// Abstract base class for media type handlers. Subclass this to implement a new media type.
	/// Subclasses found in extension assemblies are automatically registered as handlers.
	/// </summary>
	public abstract class MediaTypeHandler
	{
		/// <summary>
		/// The allowed file extensions for this media file type (lower case, with leading '.').
		/// </summary>
		public abstract IEnumerable<string> Extensions { get; }

		/// <summary>
		/// A description of this media type. (TODO: localize)
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Tests, whether the given URI can be loaded with this handler.
		/// </summary>
		/// <param name="uri">The URI to test.</param>
		/// <returns>
		/// A number that is negative if this URI can not be loaded,
		/// or otherwise indicates the priority with which this handler want to handle the URI.
		/// </returns>
		public abstract int Test(Uri uri);

		/// <summary>
		/// Handles the URI.
		/// </summary>
		/// <param name="uri">The URI to handle.</param>
		/// <param name="options">Dictionary containing additional options (may be null).</param>
		/// <returns>The loaded media object.</returns>
		public abstract Media Handle(Uri uri, Dictionary<string, string> options);

		/// <summary>
		/// Tests, whether the given URIs can be loaded at once.
		/// </summary>
		/// <param name="uris">The URIs to test.</param>
		/// <returns>
		/// A number that is negative if these URIs can not be loaded at once,
		/// or otherwise indicates the priority with which this handler want to handle the URIs.
		/// </returns>
		public virtual int TestMultiple(IEnumerable<Uri> uris)
		{
			return -1;
		}

		/// <summary>
		/// Handles multiple URIs at once. Only allowed after TestMultiple() was
		/// called on the same set of URIs.
		/// </summary>
		/// <param name="uris">The URIs to handle.</param>
		/// <returns>The loaded media object(s).</returns>
		public virtual IEnumerable<Media> HandleMultiple(IEnumerable<Uri> uris)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Helper method to check if the uri has an extension that is in the list
		/// of this handler's supported extensions.
		/// </summary>
		/// <param name="uri">The uri to check</param>
		/// <returns><c>true</c> if the extension is supported, <c>false</c>otherwise.</returns>
		protected bool CheckExtension(Uri uri)
		{
			return Extensions.Contains(uri.GetExtension().ToLower());
		}
	}
}
