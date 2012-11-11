﻿/*
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
		public virtual BackgroundDirectory Root
		{
			get
			{
				return new BackgroundDirectory(this, "/");
			}
		}

		/// <summary>
		/// Gets the file specified by a path. The path has to start with '/' and use
		/// '/' as directory separator.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The background file instance.</returns>
		/// <exception cref="FileNotFoundException">The file was not found.</exception>
		public abstract BackgroundFile GetFile(string path);

		/// <summary>
		/// Gets all available background files in a specified directory.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <returns>
		/// A list of background files.
		/// </returns>
		public abstract IEnumerable<BackgroundFile> GetFiles(BackgroundDirectory directory);

		/// <summary>
		/// Gets all subdirectories of a specified directory.
		/// </summary>
		/// <param name="parent">The parent directory.</param>
		/// <returns>
		/// A list of subdirectories.
		/// </returns>
		public abstract IEnumerable<BackgroundDirectory> GetDirectories(BackgroundDirectory parent);

		/// <summary>
		/// Gets a instance of <see cref="BackgroundDirectory"/> from a path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The directory.</returns>
		public virtual BackgroundDirectory GetDirectory(string path)
		{
			return new BackgroundDirectory(this, path);
		}

		/// <summary>
		/// Gets the URI of a background file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>The file's URI.</returns>
		public abstract Uri GetFileUri(BackgroundFile file);

		/// <summary>
		/// Gets the URI of the preview of a background file, if available.
		/// If it's not available, this will return the same as GetFileUri().
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>The URI of a preview of the file.</returns>
		public abstract Uri GetPreviewUri(BackgroundFile file);

		/// <summary>
		/// Gets the file specified by a <see cref="SongBackground"/> instance.
		/// The background's IsImage property must be <c>true</c>.
		/// </summary>
		/// <param name="background">The background to get the file for.</param>
		/// <returns>The background file.</returns>
		public BackgroundFile GetFile(Songs.SongBackground background)
		{
			if (!background.IsFile)
				throw new ArgumentException("background is not a file");

			return GetFile("/" + background.FilePath.Replace('\\', '/'));
		}
	}
}
