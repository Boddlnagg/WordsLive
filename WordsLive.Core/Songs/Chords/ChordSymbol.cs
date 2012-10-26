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
using System.Collections.Generic;
using System.Text;

namespace WordsLive.Core.Songs.Chords
{
	/// <summary>
	/// Represents a single chord symbol that maintains
	/// its name and position in the original text.
	/// </summary>
	public class ChordSymbol
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ChordSymbol"/> class.
		/// </summary>
		/// <param name="name">The chord's name.</param>
		/// <param name="position">The chord's original position in the text.</param>
		public ChordSymbol(string name, int position)
		{
			Name = name;
			Position = position;
		}

		/// <summary>
		/// Gets the chord's name.
		/// </summary>
		public String Name { get; private set; }

		/// <summary>
		/// Gets the chord's original position in the text.
		/// </summary>
		public int Position { get; private set; }

		/// <summary>
		/// Converts the chord symbol back to a <see cref="System.String"/> by wrapping its name in square brackets again.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "[" + Name + "]";
		}

		/// <summary>
		/// Gets all notes in the chord name.
		/// </summary>
		/// <example>
		/// When the name is 'D/F#' this returns 'D' and 'F#'.
		/// </example>
		/// <returns>All notes in the chord name.</returns>
		public IEnumerable<NoteSymbol> GetNotes()
		{
			string str = Name;
			string rest;
			Note n;
			int relativePosition;
			int absolutePosition = 0;

			while((n = Note.Parse(str, out rest, out relativePosition)) != null)
			{
				int relativeEnd = str.Length - rest.Length;
				yield return new NoteSymbol(n, absolutePosition + relativePosition, str.Substring(relativePosition, relativeEnd - relativePosition));
				absolutePosition += relativeEnd;
				str = rest;
			}
		}

		/// <summary>
		/// Transposes this chord symbol to another key.
		/// </summary>
		/// <param name="originalKey">The original key to transpose from.</param>
		/// <param name="amount">The amount of semitones to transpose by. Can be positive or negative.</param>
		/// <returns>The transposed chord symbol.</returns>
		public ChordSymbol Transpose(Key originalKey, int amount)
		{
			Key newKey = originalKey.Transpose(amount);
			return ReplaceNotes((note) => note.Transpose(amount).ToString(newKey));
		}

		/// <summary>
		/// Replaces all notes in the chord by applying a function.
		/// </summary>
		/// <param name="func">A function that returns a new string for every <see cref="Note"/> in the text.</param>
		/// <returns>A new chord symbol with all notes replaced.</returns>
		public ChordSymbol ReplaceNotes(Func<Note, string> func)
		{
			StringBuilder sb = new StringBuilder();

			int start = 0;

			foreach (var n in GetNotes())
			{
				string replace = func(n.Note);
				sb.Append(Name.Substring(start, n.Position - start));
				sb.Append(replace);
				start = n.Position + n.OriginalName.Length;
			}

			sb.Append(Name.Substring(start));
			return new ChordSymbol(sb.ToString(), Position);
		}
	}
}
