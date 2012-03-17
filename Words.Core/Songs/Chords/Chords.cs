using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Words.Core.Songs.Chords
{
	public class Chords
	{
		public static bool GermanNotation { get; set; }
		public static bool LongChordNames { get; set; }

		public static IEnumerable<ChordSymbol> GetChords(string text)
		{
			string rest = text;

			int i;
			int pos = 0;

			while ((i = rest.IndexOf('[')) != -1)
			{
				string before = rest.Substring(0, i);
				int end = rest.IndexOf(']', i);
				if (end < 0)
					break;

				int next = rest.IndexOf('[', i + 1);
				int linebreak = rest.IndexOf('\n', i + 1);
				if (next >= 0 && next < end || linebreak >= 0 && linebreak < end)
				{
					rest = rest.Substring(i + 1);
					pos += i + 1;
					continue;
				}

				pos += i;

				yield return new ChordSymbol(rest.Substring(i + 1, end - (i + 1)), pos);

				rest = rest.Substring(end + 1);
				pos += -i + end + 1;
			}
		}

		public static string ReplaceChords(string text, Func<ChordSymbol, string> func)
		{
			StringBuilder sb = new StringBuilder();

			int start = 0;

			foreach (ChordSymbol ch in GetChords(text))
			{
				string replace = func(ch);
				sb.Append(text.Substring(start, ch.Position - start));
				if (replace != null)
				{
					sb.Append("[");
					sb.Append(replace);
					sb.Append("]");
				}
				start = ch.Position + ch.Chord.Length + 2;
			}

			sb.Append(text.Substring(start));

			return sb.ToString();
		}

		public static string RemoveAll(string text)
		{
			return ReplaceChords(text, (chord) => null);
		}

		public static string PrettyPrint(string text)
		{
			return ReplaceChords(text, (chord) => chord.Chord.Replace('b', '♭').Replace('#', '♯'));
		}

		public static string Transpose(string text, Key originalKey, int amount)
		{
			return ReplaceChords(text, (chord) => chord.Transpose(originalKey, amount).Chord);
		}
	}
}
