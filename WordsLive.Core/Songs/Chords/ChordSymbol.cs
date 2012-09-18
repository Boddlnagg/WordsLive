using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordsLive.Core.Songs.Chords
{
	public class ChordSymbol
	{
		public class NoteSymbol
		{
			public NoteSymbol(Note note, int position, string originalString)
			{
				Note = note;
				Position = position;
				OriginalString = originalString;
			}

			public Note Note { get; private set; }
			public int Position { get; private set; }
			public string OriginalString { get; private set; }
		}

		public ChordSymbol(string chord, int position)
		{
			Chord = chord;
			Position = position;
		}

		public String Chord { get; private set; }
		public int Position { get; private set; }

		public override string ToString()
		{
			return "[" + Chord + "]";
		}

		public IEnumerable<NoteSymbol> GetNotes()
		{
			string str = Chord;
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

		public ChordSymbol Transpose(Key originalKey, int amount)
		{
			Key newKey = originalKey.Transpose(amount);
			return ReplaceNotes((note) => note.Transpose(originalKey, amount).ToString(newKey));
		}

		public ChordSymbol ReplaceNotes(Func<Note, string> func)
		{
			StringBuilder sb = new StringBuilder();

			int start = 0;

			foreach (var n in GetNotes())
			{
				string replace = func(n.Note);
				sb.Append(Chord.Substring(start, n.Position - start));
				sb.Append(replace);
				start = n.Position + n.OriginalString.Length;
			}

			sb.Append(Chord.Substring(start));
			return new ChordSymbol(sb.ToString(), Position);
		}
	}
}
