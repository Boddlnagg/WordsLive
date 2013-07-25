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
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WordsLive.Core.Songs.IO
{
	public class OpenSongSongReader : ISongReader
	{
		private static Regex wordRegex = new Regex("[^\\s]+", RegexOptions.Compiled);

		public void Read(Song song, Stream stream)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (stream == null)
				throw new ArgumentNullException("stream");

			song.LoadTemplate();

			var doc = XDocument.Load(stream);

			song.Order.Clear();
			song.Parts.Clear();

			if (doc.Root.Name != "song")
			{
				throw new SongFormatException("File is not a valid OpenSong song.");
			}

			var root = doc.Root;

			song.Title = root.Elements("title").Single().Value;
			song.Copyright = root.Elements("author").Any() ? root.Elements("author").Single().Value : "";
			if (root.Elements("copyright").Any())
				song.Copyright += "\n© " + root.Elements("copyright").Single().Value;

			song.Category = root.Elements("theme").Single().Value;
			var lines = root.Elements("lyrics").Single().Value.Split('\n');

			string partKey = null;
			LineGroup currentLineGroup = null;
			List<LineGroup> partLineGroups = null;

			for (int l = 0; l < lines.Length; l++)
			{
				var line = lines[l];

				if (line.StartsWith("[")) // next part (or group of parts)
				{
					if (partKey != null)
						FinishPart(song, partKey, partLineGroups, currentLineGroup);

					var i = line.IndexOf("]");
					if (i < 0)
						throw new SongFormatException("File is not a valid OpenSong song: Invalid part declaration.");

					partKey = line.Substring(1, i - 1).Trim();
					partLineGroups = new List<LineGroup>();
					currentLineGroup = null;
				}
				else // not the start of a new part
				{
					if (line.Trim() == String.Empty || line[0] == ';' || line.StartsWith("---"))
					{
						// ignore empty line, comments, '---' breaks (whatever they mean)
						continue;
					}

					if (partKey == null) // no part has been started -> create an anonymous one
					{
						partKey = "Unnamed";
						partLineGroups = new List<LineGroup>();
					}
					
					if (line[0] == '.')
					{
						// chord line -> always start new line group and set chords property
						if (currentLineGroup != null)
							partLineGroups.Add(currentLineGroup);

						currentLineGroup = new LineGroup { Chords = line.Substring(1) };
					}
					else if (line[0] == ' ')
					{
						// lyrics line -> set lyrics to current LineGroup
						if (currentLineGroup == null || currentLineGroup.Lines.Count == 0)
						{
							if (currentLineGroup == null)
							{
								currentLineGroup = new LineGroup();
							}
							currentLineGroup.Lines.Add(new Line { Text = line.Substring(1) });
							partLineGroups.Add(currentLineGroup);
							currentLineGroup = null;
						}
						else
						{
							throw new SongFormatException("File is not a valid OpenSong song: Expected verse number.");
						}
					}
					else if (char.IsDigit(line[0]))
					{
						int vnum = int.Parse(line[0].ToString());

						if (currentLineGroup == null)
						{
							currentLineGroup = new LineGroup();
						}
						else if (currentLineGroup.Lines.Count > 0 && vnum <= currentLineGroup.Lines.Last().Number)
						{
							partLineGroups.Add(currentLineGroup);
							currentLineGroup = new LineGroup();
						}

						currentLineGroup.Lines.Add(new Line { Text = line.Substring(1), Number = vnum });
					}
					else
					{
						throw new SongFormatException("File is not a valid OpenSong song: Expected one of ' ', '.', ';' , '[0-9]'");
					}
				}
			}

			if (partKey != null)
				FinishPart(song, partKey, partLineGroups, currentLineGroup);

			// parse order
			if (root.Elements("presentation").Any() && root.Elements("presentation").Single().Value.Trim() != String.Empty)
			{
				var val = root.Elements("presentation").Single().Value.Trim();
				var split = wordRegex.Matches(val).Cast<Match>();
				song.SetOrder(split.Select(m => GetPartName(m.Value)), ignoreMissing: true);
			}
			else
			{
				// if no order is specified, add each part once in order
				foreach (SongPart part in song.Parts)
				{
					song.Order.Add(new SongPartReference(part));
				}
			}
		}

		private void FinishPart(Song song, string key, List<LineGroup> lineGroups, LineGroup lastLineGroup)
		{
			if (lastLineGroup != null)
				lineGroups.Add(lastLineGroup);

			if (lineGroups.Count == 0)
				throw new SongFormatException("File is not a valid OpenSong song: Empty part");

			foreach (var lg in lineGroups)
			{
				if (lg.Lines.Count == 0)
					lg.Lines.Add(new Line { Text = "" });
			}

			var noNumbers = !lineGroups[0].Lines[0].Number.HasValue;

			if (noNumbers && lineGroups.Any(lg => lg.Lines.Any(l => l.Number.HasValue)))
				throw new SongFormatException("File is not a valid OpenSong song: Found mixed numbered and unnumbered lines.");

			int maxVerseNumber;
			if (noNumbers)
			{
				maxVerseNumber = 1;
			}
			else
			{
				maxVerseNumber = lineGroups.Max(lg => lg.Lines.Max(l => l.Number.Value));
			}

			for (int i = 1; i <= maxVerseNumber; i++)
			{
				if (!noNumbers && !lineGroups.Any(lg => lg.Lines.Any(l => l.Number == i)))
					continue;

				string name;
				if (noNumbers)
					name = GetPartName(key);
				else
					name = GetPartName(key + i.ToString());

				var part = new SongPart(song, name);
				var slide = new SongSlide(song);
				slide.Text = String.Join("\n", lineGroups.
					Where(lg => lg.Lines.Any(l => noNumbers || l.Number == i)).
					Select(lg => PrepareLine(lg.Lines.Where(l => noNumbers || l.Number == i).Single().Text, lg.Chords)));
				part.AddSlide(slide);

				// apply slide breaks
				int ind;
				while ((ind = slide.Text.IndexOf("||")) >= 0)
				{
					slide.Text = slide.Text.Remove(ind, 2);
					part.SplitSlide(slide, ind);
				}

				// apply line breaks
				foreach (var s in part.Slides)
				{
					s.Text = s.Text.Replace("|", "\n");
				}

				song.AddPart(part);
			}
		}

		private string PrepareLine(string text, string chords)
		{
			var spaceRegex = new Regex("\\s+");
			if (!String.IsNullOrEmpty(chords))
			{
				int offset = 0;
				foreach (Match match in wordRegex.Matches(chords))
				{
					int index = match.Index + offset;
					if (text.Length <= index)
					{
						text += new string(' ', index - text.Length + 1); // append spaces if it's too short
					}
					var chord = "[" + match.Value + "]";
					text = text.Insert(match.Index + offset, chord);
					offset += chord.Length;
				}
			}
			text = spaceRegex.Replace(text, " "); // replace multiple spaces with a single one
			text = text.Replace("_", ""); // remove underscores
			return text;
		}

		private string GetPartName(string key)
		{
			string name;

			if (key.Length <= 2)
			{
				if (key.Length == 2 && !char.IsDigit(key[1]))
					return key;

				switch (key[0])
				{
					case 'C': name = "Chorus"; break;
					case 'B': name = "Bridge"; break;
					case 'P': name = "Pre-Chorus"; break;
					case 'T': name = "Tag"; break;
					case 'V': name = "Verse"; break;
					default:
						return key; // don't know what to do with it, just return the key
				}

				return key.Length == 2 ? name + " " + key[1] : name;
			}
			else
			{
				return key;
			}
		}

		private class LineGroup
		{
			public string Chords { get; set; }
			public List<Line> Lines { get; private set; }

			public LineGroup()
			{
				Lines = new List<Line>();
			}
		}

		private class Line
		{
			public string Text { get; set; }
			public int? Number { get; set; }
		}
	}
}
