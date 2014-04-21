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

namespace WordsLive.Core.Songs.IO
{
	public class SongBeamerSongReader : ISongReader
	{
		public bool NeedsTemplate
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Reads the song data from a stream.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <param name="stream">The stream.</param>
		public void Read(Song song, Stream stream)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (stream == null)
				throw new ArgumentNullException("stream");

			using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default, true))
			{
				SongPart currentPart = null;
				string currentText = null;
				string currentTrans = null;

				string line;
				Dictionary<string, string> properties = new Dictionary<string, string>();
				int langcount = 1;
				int linenum = 0;

				while ((line = reader.ReadLine()) != null)
				{
					if (currentPart == null)
					{
						line = line.Trim();
						if (line.StartsWith("#"))
						{
							int i = line.IndexOf('=');
							string key = line.Substring(1, i - 1).ToLower();
							string value = line.Substring(i + 1);
							properties.Add(key, value);
						}
						else if (line == "---")
						{
							PreProcessSongBeamerProperties(song, properties, out langcount); // langcount > 2 is not supported (text will be ignored)
							currentPart = new SongPart(song, FindUnusedPartName(song));
						}
					}
					else
					{
						if (line == "---")
						{
							currentPart.Slides.Add(new SongSlide(song) { Size = song.Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
							currentText = null;
							song.Parts.Add(currentPart);
							currentPart = new SongPart(song, FindUnusedPartName(song));
							linenum = 0;
						}
						else if (line == "--" || line == "--A")
						{
							currentPart.Slides.Add(new SongSlide(song) { Size = song.Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
							currentText = "";
							linenum = 0;
						}
						else
						{
							if (currentText == null) // at the beginning of a new part
							{
								string name;
								if (IsSongBeamerPartName(line, out name))
								{
									currentPart.Name = name;
									currentText = "";
									linenum = 0;
								}
								else
								{
									currentText = line;
									linenum = 1;
								}
							}
							else
							{
								if (linenum % langcount == 0) // add line to text
								{
									if (linenum == 0)
										currentText = line;
									else
										currentText += "\n" + line;
								}
								else if (linenum % langcount == 1) // add line to translation
								{
									if (linenum == 1)
										currentTrans = line;
									else
										currentTrans += "\n" + line;
								}

								linenum++;
							}
						}
					}
				}

				currentPart.Slides.Add(new SongSlide(song) { Size = song.Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
				song.Parts.Add(currentPart);

				PostProcessSongBeamerProperties(song, properties);
			}
		}

		/// <summary>
		/// Helper function to generate an unused part name.
		/// </summary>
		/// <param name="song">The song to generate the name for.</param>
		/// <returns>A part name that is guaranteed to be unused in that song.</returns>
		private static string FindUnusedPartName(Song song)
		{
			int i = 1;
			while (song.FindPartByName(i.ToString()) != null)
			{
				i++;
			}
			return i.ToString();
		}

		/// <summary>
		/// Checks whether the given part name is a SongBeamer part name
		/// (see http://wiki.songbeamer.de/index.php?title=Song#Vers_Marker).
		/// </summary>
		/// <param name="value">The part name to check.</param>
		/// <returns>
		///   <c>true</c> if the specified value is a SongBeamer part name; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsSongBeamerPartName(string value, out string name)
		{
			name = null;

			if (value.StartsWith("$$M="))
			{
				name = value.Substring(4);
				return true;
			}

			string[] parts = value.Split(' ');
			if (parts.Length > 2)
			{
				name = null;
				return false;
			}

			switch (parts[0].ToLower())
			{
				case "unbekannt":
				case "unbenannt":
				case "unknown":
				case "intro":
				case "vers":
				case "verse":
				case "strophe":
				case "pre-bridge":
				case "bridge":
				case "misc":
				case "pre-refrain":
				case "refrain":
				case "pre-chorus":
				case "chorus":
				case "pre-coda":
				case "zwischenspiel":
				case "interlude":
				case "coda":
				case "ending":
				case "teil":
				case "part":
					name = value;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Helper function to process the imported properties before the actual song content is loaded.
		/// </summary>
		/// <param name="song">The imported song.</param>
		/// <param name="properties">A dictionary with properties.</param>
		/// <param name="langcount">Will be assigned the number of languages as indicated by the 'langcount' property.</param>
		private static void PreProcessSongBeamerProperties(Song song, Dictionary<string, string> properties, out int langcount)
		{
			/* We ignore the following properties:
				properties["editor"] // name of editing program 	#Editor=SongBeamer 4.04c
				properties["keywords"] // search keywords 	#Keywords=Shout to the north and the south Men of faith rise up and sing
				properties["quickfind"] // quick find string like STTN for “Shout to the North” 	#QuickFind=STTN
				properties["churchsongid"] // internal ID 	#ChurchSongID=376
				properties["comments"] // same as Comment?
				properties["version"] // format version? 99.9% it’s 3 otherwise 2 	#Version=3
			 */

			/* We currently also ignore any formatting/display properties:
				properties["backgroundimage"] // Path to background relative to image folder. Observed file types: JPEG, PNG, BMP, WMV, AVI 	#BackgroundImage=ThreeCrossesOnAHill.jpg
				properties["font"] // font for lyrics 	#Font=Arial
				properties["fontlang2"] // font for other lyrics 	#FontLang2=Arial
				properties["fontsize"] // font size for lyrics 	#FontSize=35
				properties["textalign"] // text alignment 	#TextAlign=Left
				properties["titlealign"] // title alignment 	#TitleAlign=Center
			 */

			/* We could (should) add support for the following properties in the future:
				properties["author"] // name of the lyrics author 	#Author=Sarah Full Adams (1805-1848)
				properties["melody"] // name of composer 	#Melody=Lowell Mason (1792-1872)
				properties["translation"] // name of translation author 	#Translation=Deutsch: Daniel Jacob
				properties["natcopyright"] // national copyright information 	#NatCopyright=Hänssler Verlag
				properties["rights"] // additional rights (e.g. for translated lyrics) 	#Rights=D, A, CH, FL, und L Projektion J Musikverlag, Asslar
				properties["addcopyrightinfo"] // additional copyright information 	#AddCopyrightInfo=1977
				properties["bible"] // bible passage the text is based on 	#Bible=Ps. 144,1-2
				properties["key"] // mus. key of the song 	#Key=F#m
				properties["tempo"]
				properties["titlelang[2...n]"] // song title in other language 	#TitleLang2=Näher mein Gott zu Dir
				properties["otitle"] // original title 	#OTitle=Nearer my God to Thee
				*/

			int ccli; // CCLI number 	#CCLI=858299
			if (properties.ContainsKey("ccli") && int.TryParse(properties["ccli"], out ccli))
				song.CcliNumber = ccli;

			if (properties.ContainsKey("title")) // song title 	#Title=Nearer My God To Thee
				song.Title = properties["title"];

			if (properties.ContainsKey("(c)")) // copyright information 	#(c)=1999 Hillsong Music, Australia / Kingsway’s Thankyou Music
				song.Copyright = properties["(c)"];

			if (properties.ContainsKey("categories")) // categories the song is about 	#Categories=grace, worship
				song.Category = properties["categories"];

			if (properties.ContainsKey("comment")) // arbitrary comment 	#Comment=correct slide 4!!!
				song.Comment = properties["comment"];

			if (properties.ContainsKey("songbook")) // also written: SongBook 	#Songbook=Come to Worship 1+2 / 136, In Love With Jesus 2 / 86
				song.SetSources(properties["songbook"].Split(','));

			if (properties.ContainsKey("langcount")) // number of translations in song file 	#LangCount=1
				langcount = int.Parse(properties["langcount"]);
			else
				langcount = 1;

			if (properties.ContainsKey("lang")) // language of the song 	#Lang=E
				song.Language = properties["lang"]; // TODO: what format is this in?

			// TODO: is there a "lang2" property for the language of the first translation? -> Put it in song.TranslationLanguage
		}

		/// <summary>
		/// Helper function to process the imported properties after the actual song content is loaded.
		/// </summary>
		/// <param name="song">The imported song.</param>
		/// <param name="properties">A dictionary with properties.</param>
		private static void PostProcessSongBeamerProperties(Song song, Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("verseorder"))
			{
				song.SetOrder(properties["verseorder"].Split(','), ignoreMissing: true);
			}
			else
			{
				// if no verseorder is specified, add each part once in order
				foreach (SongPart part in song.Parts)
				{
					song.Order.Add(new SongPartReference(part));
				}
			}
		}
	}
}
