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

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a reference to a song part.
	/// </summary>
	public struct SongPartReference
	{
		/// <summary>
		/// The name of the referenced <see cref="SongPart"/>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPartReference"/> class.
		/// </summary>
		/// <param name="name">The name of the referenced <see cref="SongPart"/>.</param>
		public SongPartReference(string name) : this()
		{
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPartReference"/> class.
		/// </summary>
		/// <param name="part">The referenced part.</param>
		public SongPartReference(SongPart part) : this(part.Name) { }

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SongPartReference))
				return false;

			return (((SongPartReference)obj).Name == this.Name);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}
	}
}
