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
using System.ComponentModel;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a reference to a song part.
	/// </summary>
	[JsonConverter(typeof(Json.JsonSongPartReferenceConverter))]
	public class SongPartReference
	{
		private Song root;
		private SongPart part;

		/// <summary>
		/// Gets the referenced <see cref="SongPart"/>.
		/// </summary>
		public SongPart Part
		{
			get
			{
				return part;
			}
		}

		/// <summary>
		/// Gets the index of this part reference in the song's list of parts.
		/// </summary>
		public int PartIndex
		{
			get
			{
				return root.Parts.IndexOf(part);
			}
		}

		/// <summary>
		/// Gets the index of this part reference in the songs's order.
		/// </summary>
		public int OrderIndex
		{
			get
			{
				return root.Order.IndexOf(this);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPartReference"/> class.
		/// </summary>
		/// <param name="name">The name of the referenced <see cref="SongPart"/>.</param>
		public SongPartReference(Song root, string name) : this(root.FindPartByName(name)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPartReference"/> class.
		/// </summary>
		/// <param name="part">The referenced part.</param>
		public SongPartReference(SongPart part)
		{
			if (part == null)
				throw new ArgumentNullException("part");

			this.root = part.Root;
			this.part = part;
		}
	}
}
