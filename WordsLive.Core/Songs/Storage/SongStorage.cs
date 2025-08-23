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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Abstract base class for song storages. Song storages are responsible for providing
	/// a list of all songs or a list filtered by certain criteria. Generic methods for filtering are
	/// implemented in this base class, but should be overridden if more specialized filtering methods
	/// are available.
	/// </summary>
	public abstract class SongStorage
	{
		// TODO: make API asynchronous

		/// <summary>
		/// Gets all available songs the provider can provide.
		/// </summary>
		/// <returns>All available songs.</returns>
		public abstract IEnumerable<SongData> All();

		/// <summary>
		/// Gets all available songs the provider can provide asynchronously.
		/// </summary>
		/// <returns>All available songs.</returns>
		public abstract Task<IEnumerable<SongData>> AllAsync();

		/// <summary>
		/// Filters the songs by title.
		/// </summary>
		/// <param name="query">The query to use. This is case-insensitive.</param>
		/// <returns>All songs whose title contains the query.</returns>
		public virtual IEnumerable<SongData> WhereTitleContains(string query)
		{
			return All().Where(d => d.SearchTitle.ContainsIgnoreCase(query));
		}

		/// <summary>
		/// Filters the songs using a full-text search.
		/// </summary>
		/// <param name="query">The query to use. This is case-insensitive.</param>
		/// <returns>All songs whose text, translation, or title contains the query.</returns>
		public virtual IEnumerable<SongData> WhereTextContains(string query)
		{
			return All().Where(d => d.SearchTitle.ContainsIgnoreCase(query) || d.SearchText.ContainsIgnoreCase(query) || d.SearchTranslation.ContainsIgnoreCase(query));
		}

		/// <summary>
		/// Filters the songs by source.
		/// </summary>
		/// <param name="query">The query to use. This is case-insensitive.</param>
		/// <returns>All songs whose source contains the query.</returns>
		public virtual IEnumerable<SongData> WhereSourceContains(string query)
		{
			return All().Where(d => d.Sources.ContainsIgnoreCase(query));
		}

		/// <summary>
		/// Filters the songs by copyright.
		/// </summary>
		/// <param name="query">The query to use. This is case-insensitive.</param>
		/// <returns>All songs whose copyright information contains the query.</returns>
		public virtual IEnumerable<SongData> WhereCopyrightContains(string query)
		{
			return All().Where(d => d.Copyright.ContainsIgnoreCase(query));
		}

		/// <summary>
		/// Gets the number of available songs.
		/// </summary>
		/// <returns>The number of songs.</returns>
		public virtual int Count()
		{
			return All().Count();
		}

		public abstract SongStorageEntry Get(string name);

		public abstract Task<SongStorageEntry> GetAsync(string name, CancellationToken cancellation);

		public abstract FileTransaction Put(string name);

		/// <summary>
		/// Delete a song. If the song doesn't exist, no Exception is thrown.
		/// </summary>
		/// <param name="name">The song's name.</param>
		public abstract void Delete(string name);

		public abstract FileInfo GetLocal(string name);

		public abstract bool Exists(string name);

		/// <summary>
		/// Check if the given name of the song is allowed (independent of whether it already exists
		/// or not; checks the general format and checks especially for invalid characters).
		/// </summary>
		/// <param name="name">The name of the song.</param>
		/// <returns>True if the song name can be used for saving a song.</returns>
		public abstract bool IsValidName(string name);

		//public Song Read(string name, ISongReader reader)
		//{
		//	throw new NotImplementedException();
		//}

		//public void Write(Song song, string name, ISongWriter writer)
		//{
		//	throw new NotImplementedException();
		//}

		/// <summary>
		/// Try to rewrite the given URI to use the song:// schema if it is served by this
		/// storage implementation.
		/// </summary>
		/// <param name="uri">The URI to rewrite.</param>
		/// <returns>A rewritten URI or the original unchanged URI.</returns>
		public virtual Uri TryRewriteUri(Uri uri)
		{
			return uri;
		}
	}
}
