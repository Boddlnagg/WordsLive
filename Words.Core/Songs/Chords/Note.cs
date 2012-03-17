using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Words.Core.Songs.Chords
{
	public class Note
	{
		private Note(int semitonesFromC)
		{
			SemitonesFromC = NormalizeSemitones(semitonesFromC);
		}

		public int SemitonesFromC { get; private set; }

		public static int NormalizeSemitones(int value)
		{
			value = value % 12;
			return value < 0 ? value + 12 : value;
		}

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

		public Note Transpose(Key originalKey, int amount)
		{
			return new Note(this.SemitonesFromC + amount);
		}

		public override string ToString()
		{
			return ToString(new Key("C"));
		}

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
	}
}
