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
using System.IO;

namespace WordsLive.Core.Data
{
	/// <summary>
	/// Interface for media data providers. Media data providers are responsible for
	/// turning a media location path into a Stream to be read by the media implementation.
	/// </summary>
	public interface IMediaDataProvider
	{
		/// <summary>
		/// Gets the resource at the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The resource as a stream.</returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		Stream Get(string path);

		/// <summary>
		/// Gets an absolute Uri of the resource.
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		/// <returns>A Uri pointing to the resource.</returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		Uri GetUri(string path);

		/// <summary>
		/// Gets the resource as a local file. If it actually is not a local file,
		/// it is temporarily cached locally. Don't write to this file!
		/// </summary>
		/// <param name="path">The path to the resource.</param>
		/// <returns>The resource as a local file.</returns>
		/// <exception cref="FileNotFoundException">The resource was not found.</exception>
		FileInfo GetLocal(string path);
	}
}
