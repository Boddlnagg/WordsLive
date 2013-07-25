/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// Writer for HTML export of songs (unfinished, but working).
	/// </summary>
	public class HtmlSongWriter : ISongWriter
	{
		private List<string> printedParts;

		/// <summary>
		/// Gets or sets a value indicating whether to print chords in the output or not.
		/// </summary>
		public bool PrintChords { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="HtmlSongWriter"/> class.
		/// </summary>
		public HtmlSongWriter()
		{
			PrintChords = true; // print chords by default
		}

		/// <summary>
		/// Exports a song to an HTML file.
		/// </summary>
		/// <param name="song">The song to export.</param>
		/// <param name="stream">The stream to write to.</param>
		public void Write(Song song, Stream stream)
		{
			printedParts = new List<string>();

			var doc = new XDocument(
				new XElement("html",
					new XElement("head",
						new XElement("title", song.Title),
						new XElement("style", style)
						),
					new XElement("body",
						new XElement("h1", song.Title),
						from partRef in song.Order select ExportPart(partRef.Part, PrintChords),
						new XElement("p",
							new XAttribute("id", "copyright"),
							NewlineToBr(song.Copyright))
						)
					)
				);
			doc.Save(stream);
		}

		private IEnumerable<XElement> ExportPart(SongPart part, bool printChords)
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

		private static IEnumerable<XNode> NewlineToBr(string text)
		{
			var lines = text.Split('\n');
			bool isFirst = true;
			foreach (var line in lines)
			{
				if (isFirst)
					isFirst = false;
				else
					yield return new XElement("br");

				yield return new XText(line);
			}
		}

		private IEnumerable<XNode> ParseLine(string line, bool showChords)
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

		#copyright {
			font-size: 50%;
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
