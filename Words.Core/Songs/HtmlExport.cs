using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Words.Core.Songs
{
	public static class HtmlExport
	{
		private static List<string> printedParts;

		// TODO: use this somewhere in the UI (entry in the editor's menu)
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
						from part in song.Order select ExportPart(song.FindPartByName(part), printChords),
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

		// TODO: this method is copied from SongDisplayController -> find a way to no duplicate it
		private static IEnumerable<object> ParseLine(string line, bool showChords)
		{
			string rest;

			if (String.IsNullOrEmpty(line))
				rest = String.Empty;
			else
				rest = "\uFEFF" + line.Replace(" ", " "); // not sure if we need the replace, but the \uFEFF (zero-width space)
			// makes sure that the lines starts correcty

			List<object> elements = new List<object>();

			int i;

			while ((i = rest.IndexOf('[')) != -1)
			{
				string before = rest.Substring(0, i);
				int end = rest.IndexOf(']', i);
				if (end < 0)
					break;

				int next = rest.IndexOf('[', i + 1);
				if (next >= 0 && next < end)
				{
					elements.Add(before + "[");
					rest = rest.Substring(i + 1);
					continue;
				}

				string chord = rest.Substring(i + 1, end - (i + 1));

				rest = rest.Substring(end + 1);
				elements.Add(before);

				if (showChords)
				{
					// abusing the <b> tag for chords for brevity
					// we need two nested tags, the outer one with position:relative,
					// the inner one with position:absolute (see css below)
					elements.Add(new XElement("b", new XElement("b", chord))); // TODO: pretty printing (see original method in SongDisplayController)
				}
			}

			elements.Add(rest);
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
