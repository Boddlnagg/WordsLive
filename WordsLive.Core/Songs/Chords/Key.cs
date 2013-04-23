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

namespace WordsLive.Core.Songs.Chords
{
	/// <summary>
	/// Represents a musical key.
	/// </summary>
	public class Key
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Key"/> class.
		/// </summary>
		/// <param name="name">The key's name. This is the name of the base note, optionally with an 'm' appended for minor keys.</param>
		public Key(string name)
		{
			string rest;
			int pos;
			Note = Note.Parse(name, out rest, out pos);
			if (Note == null)
				throw new ArgumentException("Invalid key name " + name);

			if ((rest.StartsWith("m") && !rest.StartsWith("maj")) || rest.ToLower().Contains("moll"))
				IsMinor = true;
		}

		/// <summary>
		/// Private constructor to be used internally.
		/// </summary>
		private Key() { }

		/// <summary>
		/// Gets the base <see cref="Note"/> for this key.
		/// </summary>
		public Note Note { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this key is a minor key.
		/// A key is always either major or minor.
		/// </summary>
		public bool IsMinor { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this key is a major key.
		/// A key is always either major or minor.
		/// </summary>
		public bool IsMajor
		{
			get
			{
				return !IsMinor;
			}
			set
			{
				IsMinor = !value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the key's signature has flats or sharps
		/// (a mix of both is impossible).
		/// </summary>
		public bool IsFlat
		{
			get
			{
				// C	0	#
				// Db	1	b
				// D	2	#
				// Eb	3	b
				// E	4	#
				// F	5	b
				//(F#   6   #)
				//     OR
				//(Gb	6	b)
				// G	7	#
				// Ab	8	b
				// A	9	#
				// Bb	10	b
				// B	11	#

				int k = Note.NormalizeSemitones(Note.SemitonesFromC + (IsMinor ? 3 : 0));

				bool wasFlat = Note.WasFlat.HasValue && Note.WasFlat.Value;

				if ((wasFlat && k > 5) || (!wasFlat && k > 6))
					k++;

				return (k % 2) == 1;
			}
		}

		/// <summary>
		/// Transposes the key by a given amount of semitones.
		/// </summary>
		/// <param name="amount">The number of semitones.</param>
		/// <returns>The transposed key.</returns>
		public Key Transpose(int amount)
		{
			return new Key { Note = Note.Transpose(amount), IsMinor = IsMinor };
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// The name of the base note with an 'm' appended if it's a minor key.
		/// </returns>
		public override string ToString()
		{
			return Note.ToString(this) + (IsMinor ? "m" : String.Empty);
		}
	}
}
