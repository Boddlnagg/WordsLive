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

namespace WordsLive.Core.Data
{
	/// <summary>
	/// Abstract base class for background data providers. Background data providers
	/// are responsible for providing a list (rather tree) the available backgrounds
	/// (images and videos) and for retrieving a single background.
	/// </summary>
	public abstract class BackgroundDataProvider
	{
		/// <summary>
		/// Gets the root directory.
		/// </summary>
		public abstract BackgroundsDirectory Root { get; }

		/// <summary>
		/// Gets all available background files in a specified directory.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <returns>
		/// A list of background filenames (relative to the specified directory).
		/// </returns>
		public abstract IEnumerable<BackgroundFile> GetFiles(BackgroundsDirectory directory);

		/// <summary>
		/// Gets all subdirectories of a specified directory.
		/// </summary>
		/// <param name="parent">The parent directory.</param>
		/// <returns>
		/// A list of subdirectories.
		/// </returns>
		public abstract IEnumerable<BackgroundsDirectory> GetDirectories(BackgroundsDirectory parent);
	}
}
