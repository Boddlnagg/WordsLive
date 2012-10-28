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
using System.Drawing;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents the text formatting options used on a song slide.
	/// </summary>
	public struct SongTextFormatting
	{
		/// <summary>
		/// Gets or sets the font size.
		/// </summary>
		public int Size;

		/// <summary>
		/// Gets or sets the font name.
		/// </summary>
		public string Name;

		/// <summary>
		/// Gets or sets a value indicating whether bold text should be used.
		/// </summary>
		public bool Bold;

		/// <summary>
		/// Gets or sets a value indicating whether italic text should be used.
		/// </summary>
		public bool Italic;

		/// <summary>
		/// Gets or sets the text color.
		/// </summary>
		public Color Color;

		/// <summary>
		/// Gets or sets the size of the outline.
		/// TODO: currently unused
		/// </summary>
		public int Outline;

		/// <summary>
		/// Gets or sets the size of the shadow.
		/// TODO: currently unused
		/// </summary>
		public int Shadow;
	}
}
