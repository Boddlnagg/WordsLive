/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WordsLive.Core.Songs.Chords;

namespace WordsLive.Core.Songs.IO
{
	public class OpenLyricsSongWriter : ISongWriter
	{
		protected readonly XNamespace ns = "http://openlyrics.info/namespace/2009/song";

		/// <summary>
		/// Writes the song data to a stream.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <param name="stream">The stream.</param>
		public void Write(Song song, Stream stream)
		{
			XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

			var versionString = "WordsLive " + Assembly.GetExecutingAssembly().GetName().Version.SimplifyVersion().ToString();
			var mappings = CreateSlideNameMappings(song);
			var sources = from s in song.Sources where !String.IsNullOrWhiteSpace(s.Songbook)
						  select new XElement(ns + "songbook",
							  new XAttribute("name", s.Songbook),
							  new XAttribute("entry", s.Number == null ? " " : s.Number.ToString())); // empty string is not allowed in entry attribute
			
			XElement root = new XElement(ns + "song", new XAttribute("version", "0.8"),
				new XAttribute("createdIn", versionString),
				new XAttribute("modifiedIn", versionString),
				new XAttribute("modifiedDate", DateTime.Now.ToString(@"yyyy-MM-ddTHH\:mm\:sszzz")),
				new XElement(ns + "properties",
					new XElement(ns + "titles", new XElement(ns + "title", song.Title)),
					String.IsNullOrWhiteSpace(song.Copyright) ? null : new XElement(ns + "copyright", song.Copyright),
					song.CcliNumber == null ? null : new XElement(ns + "ccliNo", song.CcliNumber),
					new XElement(ns + "verseOrder", String.Join(" ", from p in song.Order from s in p.Part.Slides select mappings[s])),
					sources.Any() ? new XElement(ns + "songbooks", sources) : null,
					String.IsNullOrWhiteSpace(song.Category) ? null : new XElement(ns + "themes", new XElement(ns + "theme", song.Category)),
					String.IsNullOrWhiteSpace(song.Comment) ? null : new XElement(ns + "comments", new XElement(ns + "comment", song.Comment))
				),
				new XElement(ns + "lyrics",
					// TODO: add separate <verse lang="..."> tags for translation
					//		 (what to do if there is a translation but we don't know its language?)
					from p in song.Parts from s in p.Slides select new XElement(ns + "verse",
						new XAttribute("name", mappings[s]),
						new XElement(ns + "lines", ConvertTextToNodes(s.Text))
					)
				)
			);

			doc.Add(root);

			StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
			doc.Save(writer);
			writer.Close();
		}

		private IEnumerable<XNode> ConvertTextToNodes(string text)
		{
			StringBuilder sb = new StringBuilder();

			int start = 0;

			foreach (ChordSymbol ch in Chords.Chords.GetChords(text))
			{
				foreach (var e in InsertBrElements(text.Substring(start, ch.Position - start)))
					yield return e;
				
				yield return new XElement(ns + "chord", new XAttribute("name", ch.Name));
				start = ch.Position + ch.Name.Length + 2;
			}

			foreach (var e in InsertBrElements(text.Substring(start)))
				yield return e;
		}

		private IEnumerable<XNode> InsertBrElements(string text)
		{
			bool first = true;
			foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None))
			{
				if (!first)
				{
					yield return new XElement(ns + "br");
				}
				else
				{
					first = false;
				}

				yield return new XText(line);
			}
		}

		private Dictionary<SongSlide, string> CreateSlideNameMappings(Song song)
		{
			var mappings = new Dictionary<SongSlide, string>();

			foreach (var part in song.Parts)
			{
				var name = part.Name.Trim().ToLower();
				string p = null;
				if (name[0] == 'r' || name[0] == 'c') // refrain or chorus
				{
					p = "c";
				}
				else if (name[0] == 'v' || name.StartsWith("st")) // verse or strophe
				{
					p = "v";
				}
				else if (name[0] == 'p') // pre-chorus
				{
					p = "p";
				}
				else if (name[0] == 'b') // bridge
				{
					p = "b";
				}
				else if (name[0] == 'e' || name.StartsWith("schlu")) // ending or schluss/schluß
				{
					p = "e";
				}

				if (p != null)
				{
					// add a number if there was one originally
					var match = Regex.Match(name, @"^[^\d]*(\d+)[^\d]*$");
					if (match != Match.Empty)
					{
						p += match.Groups[1].Value;
					}
				}
	
				if (p == null || mappings.ContainsValue(p))
				{
					// create name form original name (already converted to lowercase) by removing whitespace ...
					p = new Regex(@"\s*").Replace(name, string.Empty);
					// ... and replacing all special characters with '_'
					p = new Regex(@"[^a-z0-9_-]").Replace(p, "_");
				}

				if (part.Slides.Count == 1)
				{
					while (mappings.ContainsValue(p))
						p += "_"; // in case we already have that name, append '_'

					mappings.Add(part.Slides[0], p);
				}
				else
				{
					char c = 'a';
					string append = "";

					foreach (var slide in part.Slides)
					{
						string result = p + c + append;

						while (mappings.ContainsValue(result))
							result += "_"; // in case we already have that name, append '_'

						mappings.Add(slide, result);

						if (c < 'z')
							c++; // next letter for next slide (a-z)
						else
							append += "z"; // in case we have more than 26 slides, append more 'z'
					}
				}
			}

			return mappings;
		}
	}
}
