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
using System.Linq;
using System.Xml.Linq;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Static class for HTML export of songs (unfinished, but working).
	/// TODO: add entry point for this in the UI
	/// TODO: add generalized way for importing and exporting songs
	/// </summary>
	public static class HtmlExport
	{
		private static List<string> printedParts;

		/// <summary>
		/// Exports a song to a HTML file.
		/// </summary>
		/// <param name="song">The song to export.</param>
		/// <param name="file">The target HTML file.</param>
		/// <param name="printChords">Whether to include chords in the output.</param>
		public static void ExportSong(Song song, string file, bool printChords = true)
		{
			printedParts = new List<string>();

			var doc = new XDocument(
				new XElement("html",
					new XElement("head",
						new XElement("title", song.SongTitle),
						new XElement("style", style)
						),
					new XElement("body",
						new XElement("h1", song.SongTitle),
						from partRef in song.Order select ExportPart(song.FindPartByReference(partRef), printChords),
						new XElement("p",
							new XAttribute("id", "copyright"),
							song.Copyright)
						)
					)
				);
			doc.Save(file);
		}

		private static IEnumerable<XElement> ExportPart(SongPart part, bool printChords)
		{
			if (!printedParts.Contains(part.Name))
			{
				yield return new XElement("h2", part.Name);
				yield return new XElement("p",
					from line in part.Text.Split('\n') select new XElement("span", ParseLine(line, printChords))
					);
				printedParts.Add(part.Name);
			}
			else
			{
				yield return new XElement("h2", "("+part.Name+")");
			}
		}

		private static IEnumerable<XNode> ParseLine(string line, bool showChords)
		{
			string rest;

			if (String.IsNullOrEmpty(line))
				rest = String.Empty;
			else
				rest = "\uFEFF" + line.Replace(" ", " "); // not sure if we need the replace, but the \uFEFF (zero-width space)
														  // makes sure that the lines starts correcty

			var elements = new List<XNode>();
			int i = 0;

			foreach (var ch in WordsLive.Core.Songs.Chords.Chords.GetChords(rest))
			{
				elements.Add(new XText(rest.Substring(0, ch.Position - i)));

				if (showChords)
				{
					// abusing the <b> tag for chords for brevity
					// we need two nested tags, the outer one with position:relative,
					// the inner one with position:absolute (see css below)
					elements.Add(new XElement("b", new XElement("b", ch.Name)));
				}

				int delta = (ch.Position - i) + ch.Name.Length + 2;

				rest = rest.Substring(delta);
				i += delta;
			}

			elements.Add(new XText(rest));
			return elements;
		}

		private static string style = @"body {
			font-size: 18pt;
		}
		
		h1 {
			font-size: 100%;
		}
		
		span {
			display: block;
			overflow: visible;
		}
		
		p {
			margin-top: 10px;
		}
		
		h2 {
			font-style: italic;
			font-size: 70%;
			font-weight: normal;
		}
  
		/* Chords */
		b {
			font-weight: normal;
			position: relative;
		}

		b b {
			font-weight: bold;
			font-size: 65%;
			position: absolute;
			top: -120%;
		}";
	}
}
