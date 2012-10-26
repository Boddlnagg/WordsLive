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

namespace WordsLive.Core.Songs.Chords
{
	/// <summary>
	/// Represents a note symbol.
	/// </summary>
	public class NoteSymbol
	{
		/// <summary>
		/// The actual note.
		/// </summary>
		public Note Note { get; private set; }

		/// <summary>
		/// The note's position in the original string.
		/// </summary>
		public int Position { get; private set; }

		/// <summary>
		/// The original name of the note as it appeared in the parsed string.
		/// </summary>
		public string OriginalName { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NoteSymbol"/> class.
		/// </summary>
		/// <param name="note">The note.</param>
		/// <param name="position">The position in the original string.</param>
		/// <param name="originalString">The original name.</param>
		public NoteSymbol(Note note, int position, string originalName)
		{
			Note = note;
			Position = position;
			OriginalName = originalName;
		}
	}
}
