using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordsLive.Core.Songs
{
	public static class SongBeamerImport
	{
		/// <summary>
		/// Imports a song from a SongBeamer *.sng file. This takes an initialized prototype song and overwrites any imported content.
		/// </summary>
		/// <param name="prototype">The prototype.</param>
		/// <param name="filename">The filename.</param>
		public static void Import(Song prototype, string filename)
		{
			if (prototype == null)
				throw new ArgumentNullException("prototype");

			if (filename == null)
				throw new ArgumentNullException("filename");

			prototype.Order.Clear();
			prototype.Parts.Clear();

			using (StreamReader reader = new StreamReader(filename, System.Text.Encoding.Default, true))
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
							PreProcessSongBeamerProperties(prototype, properties, out langcount); // langcount > 2 is not supported (text will be ignored)
							currentPart = new SongPart() { Name = FindUnusedPartName(prototype) };
						}
					}
					else
					{
						if (line == "---")
						{
							currentPart.Slides.Add(new SongSlide { Size = prototype.Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
							currentText = null;
							prototype.Parts.Add(currentPart);
							currentPart = new SongPart { Name = FindUnusedPartName(prototype) };
							linenum = 0;
						}
						else if (line == "--" || line == "--A")
						{
							currentPart.Slides.Add(new SongSlide { Size = prototype.Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
							currentText = "";
							linenum = 0;
						}
						else
						{
							if (currentText == null) // at the beginning of a new part
							{
								if (IsSongBeamerPartName(line))
								{
									currentPart.Name = line;
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

				currentPart.Slides.Add(new SongSlide { Size = prototype.Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
				prototype.Parts.Add(currentPart);

				PostProcessSongBeamerProperties(prototype, properties);
			}
		}

		private static string FindUnusedPartName(Song song)
		{
			int i = 1;
			while (song.FindPartByName(i.ToString()) != null)
			{
				i++;
			}
			return i.ToString();
		}

		private static bool IsSongBeamerPartName(string value)
		{
			// See http://wiki.songbeamer.de/index.php?title=Song#Vers_Marker

			string[] parts = value.Split(' ');
			if (parts.Length > 2)
				return false;

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
					return true;
				default:
					return false;
			}
		}

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
				properties["ccli"] // CCLI number 	#CCLI=858299
				properties["bible"] // bible passage the text is based on 	#Bible=Ps. 144,1-2
				properties["key"] // mus. key of the song 	#Key=F#m
				properties["tempo"]
				properties["titlelang[2...n]"] // song title in other language 	#TitleLang2=Näher mein Gott zu Dir
				properties["otitle"] // original title 	#OTitle=Nearer my God to Thee
				*/

			if (properties.ContainsKey("title")) // song title 	#Title=Nearer My God To Thee
				song.SongTitle = properties["title"];

			if (properties.ContainsKey("(c)")) // copyright information 	#(c)=1999 Hillsong Music, Australia / Kingsway’s Thankyou Music
				song.Copyright = properties["(c)"];

			if (properties.ContainsKey("categories")) // categories the song is about 	#Categories=grace, worship
				song.Category = properties["categories"];

			if (properties.ContainsKey("comment")) // arbitrary comment 	#Comment=correct slide 4!!!
				song.Comment = properties["comment"];

			if (properties.ContainsKey("songbook")) // also written: SongBook 	#Songbook=Come to Worship 1+2 / 136, In Love With Jesus 2 / 86
				song.Sources = (from s in properties["songbook"].Split(',') select SongSource.Parse(s)).ToList();

			if (properties.ContainsKey("langcount")) // number of translations in song file 	#LangCount=1
				langcount = int.Parse(properties["langcount"]);
			else
				langcount = 1;

			if (properties.ContainsKey("lang")) // language of the song 	#Lang=E
				song.Language = properties["lang"]; // TODO: what format is this in?

			// TODO: is there a "lang2" property for the language of the first translation? -> Put it in song.TranslationLanguage
		}

		private static void PostProcessSongBeamerProperties(Song song, Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("verseorder"))
			{
				song.Order = properties["verseorder"].Split(',').ToList();
			}
			else
			{
				foreach (SongPart part in song.Parts)
				{
					song.Order.Add(part.Name);
				}
			}
		}
	}
}
