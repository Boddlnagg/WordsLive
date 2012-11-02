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

using System.Collections.Generic;
using System.Linq;

namespace WordsLive.Core.Data
{
	/// <summary>
	/// Abstract base class for song data providers. Song data providers are responsible for providing
	/// a list of all songs or a list filtered by certain criteria. Generic methods for filtering are
	/// implemented in this base class, but should be overridden if more specialized filtering methods
	/// are available. There is no support for a directory structure for songs, so they are identified
	/// using only their filename.
	/// </summary>
	public abstract class SongDataProvider : MediaDataProvider
	{
		/// <summary>
		/// Gets all available songs the provider can provide.
		/// </summary>
		/// <returns>All available songs.</returns>
		public abstract IEnumerable<SongData> All();

		/// <summary>
		/// Filters the songs by title.
		/// </summary>
		/// <param name="query">The query to use. This is case-insensitive.</param>
		/// <returns>All songs whose title contains the query.</returns>
		public virtual IEnumerable<SongData> WhereTitleContains(string query)
		{
			return All().Where(d => d.Title.ContainsIgnoreCase(query));
		}

		/// <summary>
		/// Filters the songs using a full-text search.
		/// </summary>
		/// <param name="query">The query to use. This is case-insensitive.</param>
		/// <returns>All songs whose text or title contains the query.</returns>
		public virtual IEnumerable<SongData> WhereTextContains(string query)
		{
			return All().Where(d => d.Title.ContainsIgnoreCase(query) || d.Text.ContainsIgnoreCase(query));
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
	}
}
