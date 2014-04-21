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

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// Importer for CCLI SongSelect files in .txt format or from clipboard
	/// Implementation inspired by http://bazaar.launchpad.net/~openlp-core/openlp/trunk/view/head:/openlp/plugins/songs/lib/cclifileimport.py
	/// </summary>
	public class CcliTxtSongReader : CcliSongReader
	{
		public override void Read(Song song, Stream stream)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (stream == null)
				throw new ArgumentNullException("stream");

			using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
			{
				string line;
				int lineType = 0;
				List<string> verseLineList = null;
				string verseType = null;
				bool checkFirstLine = false;
				string copyright = null;

				while ((line = reader.ReadLine()) != null)
				{
					var cleanLine = line.Trim();
					if (String.IsNullOrEmpty(cleanLine))
					{
						if (lineType == 0)
						{
							continue;
						}
						else if (verseLineList != null) // empty line and there were lyrics before -> create part
						{
							var part = new SongPart(song, verseType);
							var slide = new SongSlide(song);
							slide.Text = String.Join("\n", verseLineList.ToArray());
							part.AddSlide(slide);
							song.AddPart(part);
							song.AddPartToOrder(part);

							verseLineList = null;
						}
					}
					else // not an empty line
					{
						if (lineType == 0) // very first line -> song title
						{
							song.Title = cleanLine;
							lineType++;
						}
						else if (lineType == 1) // lyrics/parts
						{
							if (cleanLine.StartsWith("CCLI")) // end of lyrics, start of copyright information
							{
								lineType++;
								string num = cleanLine.Split(' ').Last();
								song.CcliNumber = int.Parse(num);
							}
							else if (verseLineList == null)
							{
								verseType = GetPartName(cleanLine, out checkFirstLine);
								verseLineList = new List<string>();
							}
							else
							{
								if (checkFirstLine)
								{
									if (!CheckFirstLine(cleanLine, ref verseType))
									{
										// add text if it was not a part name
										verseLineList.Add(line);
									}
									checkFirstLine = false;
								}
								else
								{
									verseLineList.Add(line);
								}
							}
						}
						else if (lineType == 2) // copyright information
						{
							if (copyright == null)
							{
								copyright = cleanLine;
							}
							else
							{
								copyright += "\n" + cleanLine;
							}
						}
					}
				}

				song.Copyright = copyright;
			}
		}
	}
}
