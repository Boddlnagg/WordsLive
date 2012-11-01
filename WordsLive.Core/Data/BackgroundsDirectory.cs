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
	/// Represents a directory that contains backgrounds. An instance of this class can only be obtained
	/// using an instance of <see cref="BackgroundDataProvider"/>.
	/// </summary>
	public class BackgroundsDirectory
	{
		private BackgroundDataProvider provider;

		internal BackgroundsDirectory(BackgroundDataProvider provider, BackgroundsDirectory parent, string name)
		{
			this.provider = provider;
			this.Parent = parent;
			this.Name = name;
		}

		/// <summary>
		/// Gets the name of this directory.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the parent director.
		/// </summary>
		public BackgroundsDirectory Parent { get; private set; }
		
		/// <summary>
		/// Gets a value indicating whether this is the root directory.
		/// </summary>
		public bool IsRoot
		{
			get
			{
				return Parent == null;
			}
		}

		/// <summary>
		/// Gets the subdirectories in this directory.
		/// </summary>
		public IEnumerable<BackgroundsDirectory> Directories
		{
			get
			{
				return provider.GetDirectories(this);
			}
		}

		/// <summary>
		/// Gets the actual backgrounds in this directory.
		/// </summary>
		public IEnumerable<string> Backgrounds
		{
			get
			{
				return provider.GetFiles(this);
			}
		}
	}
}
