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
using MonitoredUndo;

namespace WordsLive.Core.Songs.Chords
{
	/// <summary>
	/// Static class to deal with chords.
	/// </summary>
	public static class Chords
	{
		/// <summary>
		/// Gets or sets a value indicating whether to use the german notation
		/// (H instead of B and B instead of Bb).
		/// </summary>
		public static bool GermanNotation { get; set; }

		/// <summary>
		/// Gets or sets a value indicationg whether to use long german chord names
		/// (i.e. Fis instead of F#).
		/// </summary>
		public static bool LongChordNames { get; set; }

		/// <summary>
		/// Parses a text and returns all chord symbols in it.
		/// </summary>
		/// <param name="text">A text that contains chord symbols in square brackets.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Replaces all chords in a text by applying a function.
		/// </summary>
		/// <param name="text">The text to process.</param>
		/// <param name="func">A function that returns a new string for each <see cref="ChordSymbol"/>.
		/// When this function returns <c>null</c> the chord is removed.</param>
		/// <returns>The text with the chords replaced.</returns>
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
				start = ch.Position + ch.Name.Length + 2;
			}

			sb.Append(text.Substring(start));

			return sb.ToString();
		}

		/// <summary>
		/// Removes all chords from the text.
		/// </summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The text with all chords removed.</returns>
		public static string RemoveAll(string text)
		{
			return ReplaceChords(text, (chord) => null);
		}

		/// <summary>
		/// Pretty prints the text by replacing the 'b' and '#' characters in chords with the correct unicode symbols.
		/// </summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The resulting text.</returns>
		public static string PrettyPrint(string text)
		{
			return ReplaceChords(text, (chord) => chord.Name.Replace('b', '♭').Replace('#', '♯'));
		}

		/// <summary>
		/// Transposes all chords in a text.
		/// </summary>
		/// <param name="text">The text to process.</param>
		/// <param name="originalKey">The original key to transpose from.</param>
		/// <param name="amount">The amount of semitones to transpose by.</param>
		/// <returns>The text with all chords transposed.</returns>
		public static string Transpose(string text, Key originalKey, int amount)
		{
			return ReplaceChords(text, (chord) => chord.Transpose(originalKey, amount).Name);
		}

		/// <summary>
		/// Transpose all chords in a song (can be undone).
		/// </summary>
		/// <param name="song">The song to process.</param>
		/// <param name="originalKey">The original key to transpose from.</param>
		/// <param name="amount">The amount of semitones to transpose by.</param>
		public static void Transpose(Song song, Key originalKey, int amount)
		{
			using (new UndoBatch(song.UndoKey, "TransposeChords", false))
			{
				foreach (var part in song.Parts)
				{
					foreach (var slide in part.Slides)
					{
						slide.Text = Transpose(slide.Text, originalKey, amount);
					}
				}
			}
		}

		/// <summary>
		/// Removes all chords from a song (can be undone).
		/// </summary>
		/// <param name="song">The song to process.</param>
		public static void RemoveAll(Song song)
		{
			using (new UndoBatch(song.UndoKey, "RemoveChords", false))
			{
				foreach (var part in song.Parts)
				{
					foreach (var slide in part.Slides)
					{
						slide.Text = RemoveAll(slide.Text);
					}
				}
			}
		}
	}
}
