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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// See http://openlyrics.info/dataformat.html
	/// </summary>
	public class OpenLyricsSongReader : ISongReader
	{
		protected readonly XNamespace ns = "http://openlyrics.info/namespace/2009/song";

		public bool NeedsTemplate
		{
			get { return true; }
		}

		public void Read(Song song, Stream stream)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (stream == null)
				throw new ArgumentNullException("stream");

			var doc = XDocument.Load(stream);

			if (doc.Root.Name != ns + "song")
			{
				throw new SongFormatException("File is not a valid OpenLyrics song.");
			}

			var prop = doc.Root.Element(ns + "properties");

			song.Title = prop.Element(ns + "titles").Elements(ns + "title").First().Value;
			var authors = prop.Elements(ns + "authors");
			song.Copyright = authors.Any() ? String.Join(", ", authors.Single().Elements(ns + "author").Select(e => e.Value)) : String.Empty;
			if (prop.Elements(ns + "publisher").Any())
			{
				var publisher = "Publisher: " + prop.Element(ns + "publisher").Value;
				song.Copyright = String.IsNullOrEmpty(song.Copyright) ? publisher : song.Copyright + "\n" + publisher;
			}

			if (prop.Elements(ns + "copyright").Any())
			{
				var copyright = prop.Element(ns + "copyright").Value;
				if (!copyright.StartsWith("©") && !copyright.StartsWith("(c)"))
					copyright = "© " + copyright;

				song.Copyright = String.IsNullOrEmpty(song.Copyright) ? copyright : song.Copyright + "\n" + copyright;
			}

			song.CcliNumber = prop.Elements(ns + "ccliNo").Any() ? int.Parse(prop.Element(ns + "ccliNo").Value) : (int?)null;
			song.Category = prop.Elements(ns + "themes").Any() ? String.Join("; ", prop.Element(ns + "themes").Elements(ns + "theme").Select(e => e.Value)) : String.Empty;
			song.Comment = prop.Elements(ns + "comments").Any() ? String.Join("\n", prop.Element(ns + "comments").Elements(ns + "comment").Select(e => e.Value)) : String.Empty;

			if (prop.Elements(ns + "songbooks").Any())
			{
				int number;
				song.SetSources(prop.Element(ns + "songbooks").Elements(ns + "songbook").Select(e => new SongSource(song) {
					Songbook = e.Attribute("name").Value,
					Number = e.Attributes("entry").Any() ? (int.TryParse(e.Attribute("entry").Value, out number) ? number : (int?)null) : null
				}));
			}

			var mappings = new Dictionary<string, string>();

			foreach (var verse in doc.Root.Element(ns + "lyrics").Elements(ns + "verse"))
			{
				string originalName = verse.Attribute("name").Value;
				// add language key to part name (we don't know which one is the translation
				// and we can not automatically deal with more than 2 languages)
				string name = originalName + (verse.Attributes("lang").Any() ? " (" + verse.Attribute("lang").Value + ")" : String.Empty);

				if (!mappings.ContainsKey(originalName))
				{
					// keep a mapping from original name to name with appended language to be used for the verse order
					mappings.Add(originalName, name);
				}

				var slides = verse.Elements(ns + "lines").Select(lines =>
				{
					StringBuilder str = new StringBuilder();
					foreach (var n in lines.Nodes())
					{
						if (n is XText)
						{
							// remove whitespace around line breaks
							str.Append(new Regex(@"[\t ]*\r?\n[\t ]*").Replace((n as XText).Value, String.Empty));
						}
						else
						{
							XElement e = (XElement)n;
							if (e.Name == ns + "br")
							{
								str.AppendLine();
							}
							else if (e.Name == ns + "chord")
							{
								str.Append("[");
								str.Append(e.Attribute("name").Value);
								str.Append("]");
							}
						}
					}

					return new SongSlide(song) { Text = str.ToString() };
				});

				song.AddPart(new SongPart(song, name, slides));
			}

			if (prop.Elements(ns + "verseOrder").Any())
			{
				song.SetOrder(prop.Element(ns + "verseOrder").Value.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(n => mappings[n]));
			}
			else
			{
				// if we have no verseOrder element, use all verses from the mappings dictionary
				// (so multiple translations will appear only once)
				song.SetOrder(mappings.Values);
			}
		}
	}
}
