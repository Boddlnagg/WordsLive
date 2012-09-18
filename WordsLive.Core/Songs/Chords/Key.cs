using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordsLive.Core.Songs.Chords
{
	public class Key
	{
		public Key(string name)
		{
			string rest;
			int pos;
			Note = Note.Parse(name, out rest, out pos);
			if (Note == null)
				throw new ArgumentException("Invalid key name " + name);

			if (rest.StartsWith("m") && !rest.StartsWith("maj"))
				IsMinor = true;
		}

		private Key() { }

		public Note Note { get; private set; }
		public bool IsMinor { get; private set; }
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
				//     +1  (#)
				// Gb	6	b
				// G	7	#
				// Ab	8	b
				// A	9	#
				// Bb	10	b
				// B	11	#

				int k = Note.NormalizeSemitones(Note.SemitonesFromC + (IsMinor ? 3 : 0));

				if (k > 5)
					k++;

				return (k % 2) == 1;
			}
		}

		public Key Transpose(int amount)
		{
			return new Key { Note = Note.Transpose(this, amount), IsMinor = IsMinor };
		}

		public override string ToString()
		{
			return Note.ToString(this) + (IsMinor ? "m" : String.Empty);
		}
	}
}
