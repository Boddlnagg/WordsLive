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
	/// <summary>
	/// Bidirectional media data providers are data providers that can additionally write resources back to the storage.
	/// </summary>
	public interface IBidirectionalMediaDataProvider : IMediaDataProvider
	{
		/// <summary>
		/// Opens a transaction to put a resource at the specified path.
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		/// <param name="allowOverwrite">If set to <c>false</c>, a <see cref="FileExistsException"/> is thrown if the file already exists.</param>
		/// <returns>The file transaction.</returns>
		FileTransaction Put(string path, bool allowOverwrite);

		/// <summary>
		/// Deletes the specified resource.
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		void Delete(string path);
	}
}
