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
	/// Represents a single note (i.e. 'A', 'F#', 'Bb') indicated by the number of semitones
	/// from the note 'C'. Therefore it is independent of the key ('F#' and 'Gb' are the same notes).
	/// </summary>
	public class Note
	{
		/// <summary>
		/// Private constructor to be used internally.
		/// </summary>
		/// <param name="semitonesFromC">The number of semitones from the note 'C'.</param>
		private Note(int semitonesFromC)
		{
			SemitonesFromC = NormalizeSemitones(semitonesFromC);
		}

		/// <summary>
		/// Gets the number of semitones from tha note 'C' that defines this note.
		/// </summary>
		public int SemitonesFromC { get; private set; }

		/// <summary>
		/// Transposes this instance to another key.
		/// </summary>
		/// <param name="amount">The amount of semitones to transpose by. Can be positive or negative.</param>
		/// <returns>The transposed note.</returns>
		public Note Transpose(int amount)
		{
			return new Note(this.SemitonesFromC + amount);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// The name of this note in the key of C major.
		/// </returns>
		public override string ToString()
		{
			return ToString(new Key("C"));
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="key">The key in which the note should be represented.</param>
		/// <returns>
		/// The name of the note in the given key.
		/// </returns>
		public string ToString(Key key)
		{
			bool isFlat = key.IsFlat;
			bool longNames = Chords.LongChordNames;

			switch (NormalizeSemitones(SemitonesFromC))
			{
				case 0:
					return "C";
				case 1:
					return isFlat ? (longNames ? "Des" : "Db") : (longNames ? "Cis" : "C#");
				case 2:
					return "D";
				case 3:
					return isFlat ? (longNames ? "Es" : "Eb") : (longNames ? "Dis" : "D#");
				case 4:
					return "E";
				case 5:
					return "F";
				case 6:
					return isFlat ? (longNames ? "Ges" : "Gb") : (longNames ? "Fis" : "F#");
				case 7:
					return "G";
				case 8:
					return isFlat ? (longNames ? "As" : "Ab") : (longNames ? "Gis" : "G#");
				case 9:
					return "A";
				case 10:
					return isFlat ? (Chords.GermanNotation ? "B" : "Bb") : (longNames ? "Ais" : "A#");
				case 11:
					return Chords.GermanNotation ? "H" : "B";
				default:
					return null; // this can never happen because it's normalized
			}
		}

		/// <summary>
		/// Normalizes the semitones by limiting them to the range of 0 - 11.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int NormalizeSemitones(int value)
		{
			value = value % 12;
			return value < 0 ? value + 12 : value;
		}

		/// <summary>
		/// Parses a string in order to find the name of a note.
		/// </summary>
		/// <param name="str">The string to parse.</param>
		/// <param name="rest">Will be assigned the rest of the string after the parsed note.</param>
		/// <param name="position">Will be assigned the position at which the note name starts in the original string.</param>
		/// <returns>The parsed note or <c>null</c> if no note was found in the string.</returns>
		public static Note Parse(string str, out string rest, out int position)
		{
			if (string.IsNullOrEmpty(str))
			{
				rest = null;
				position = 0;
				return null;
			}

			char? next = str.Length > 1 ? str[1] : (char?)null;

			int? semitones = null;
			int i = 0;

			while (!semitones.HasValue && i < str.Length)
			{
				switch (str[i])
				{
					case 'C':
						semitones = 0; break;
					case 'D':
						semitones = 2; break;
					case 'E':
						semitones = 4; break;
					case 'F':
						semitones = 5; break;
					case 'G':
						semitones = 7; break;
					case 'A':
						semitones = 9; break;
					case 'H':
						semitones = 11; break;
					case 'B':
						semitones = (Chords.GermanNotation && next != 'b' && next != '♭') ? 12 : 11; break;
				}

				i++;
			}

			if (!semitones.HasValue)
			{
				rest = null;
				position = 0;
				return null;
			}
			else
			{
				position = i - 1;
			}

			if (str.Length > i)
			{
				if (str[i] == '#' || str[i] == '♯')
				{
					semitones++;
					i++;
				}
				else if (str[i] == 'b' || str[i] == '♭')
				{
					semitones--;
					i++;
				}
				else if (str[i] == 's' && (semitones == 4 || semitones == 9)) // Es and As
				{
					semitones--;
					i++;
				}
				else if (str.Length > i + 1)
				{
					var germanSign = str.Substring(i, 2);
					if (germanSign == "is")
					{
						semitones++;
						i += 2;
					}
					else if (germanSign == "es")
					{
						semitones--;
						i += 2;
					}
				}
			}

			Note n = new Note(semitones.Value);

			rest = str.Substring(i);

			return n;
		}
	}
}
