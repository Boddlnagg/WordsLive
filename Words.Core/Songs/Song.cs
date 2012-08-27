using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Words.Core.Songs
{
	public class Song : Media
	{
		public static string Template { get; set; }

		public string SongTitle { get; set; }

		public override string Title
		{
			get
			{
				return SongTitle;
			}
		}

		public string Category { get; set; }
		public string Language { get; set; }
		public string Comment { get; set; }
		public IList<SongBackground> Backgrounds { get; set; }
		public SongFormatting Formatting { get; set; }
		
		public IList<SongPart> Parts { get; set; }
		
		public IList<string> Order { get; set; }

		public string Text
		{
			get
			{
				return string.Join("\n", Parts.Select(part => part.Text).ToArray());
			}
		}

		public string TextWithoutChords
		{
			get
			{
				return string.Join("\n", Parts.Select(part => part.TextWithoutChords).ToArray());
			}
		}
		
		public string Copyright { get; set; }
	   
		public IList<SongSource> Sources { get; set; }

		public SongSlide FirstSlide
		{
			get
			{
				if (this.Order.Count == 0)
					return null;
				return (from p in this.Parts where p.Name == this.Order[0] select p.Slides[0]).Single();
			}
		}

		public SongSlide LastSlide
		{
			get
			{
				if (this.Order.Count == 0)
					return null;
				return (from p in this.Parts where p.Name == this.Order[this.Order.Count - 1] select p.Slides[p.Slides.Count - 1]).Single();
			}
		}

		public bool HasTranslation
		{
			get
			{
				return this.Parts.Any((part) => part.HasTranslation);
			}
		}

		public bool HasChords
		{
			get
			{
				return Chords.Chords.GetChords(Text).Any();
			}
		}

		public SongPart FindPartByName(string partName)
		{
			return (from p in this.Parts where p.Name == partName select p).SingleOrDefault();
		}

		public Song()
		{ }

		public Song(string filename)
		{
			this.Load(filename);
		}

		public override void Load()
		{
			this.Load(this.File);
		}

		private void Load(string filename)
		{
			FileInfo file = new FileInfo(filename);
			if (file.Extension.ToLower() == ".ppl")
			{
				LoadPowerpraise(file, false);
			}
			else if (file.Extension.ToLower() == ".sng")
			{
				LoadSongBeamer(file);
			}
			else
			{
				throw new Exception("Invalid song format");
			}
		}

		/// <summary>
		/// Only loads title and backgrounds
		/// </summary>
		/// <param name="fileName">The filename.</param>
		public override void LoadMetadata(string fileName)
		{
			base.LoadMetadata(fileName);

			FileInfo file = new FileInfo(fileName);
			if (file.Extension == ".ppl")
			{
				LoadPowerpraise(file, true);
			}
			else
			{
				throw new Exception("Invalid song format");
			}
		}

		#region Powerpraise compatibility

		private void LoadPowerpraise(FileInfo file, bool metadataOnly)
		{
			XDocument doc = XDocument.Load(file.FullName);
			XElement root = doc.Element("ppl");
			this.SongTitle = root.Element("general").Element("title").Value;

			var formatting = root.Element("formatting");
			this.Backgrounds = (from bg in root.Element("formatting").Element("background").Elements("file") select LoadPowerpraiseBackground(bg.Value)).ToList();
			
			if (!metadataOnly)
			{
				this.Formatting = new SongFormatting
				{
					MainText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("maintext")),
					TranslationText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("translationtext")),
					SourceText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("sourcetext")),
					CopyrightText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("copyrighttext")),
					TextLineSpacing = int.Parse(formatting.Element("linespacing").Element("main").Value),
					TranslationLineSpacing = int.Parse(formatting.Element("linespacing").Element("translation").Value),
					SourceBorderRight = int.Parse(formatting.Element("borders").Element("sourceright").Value),
					SourceBorderTop = int.Parse(formatting.Element("borders").Element("sourcetop").Value),
					CopyrightBorderBottom = int.Parse(formatting.Element("borders").Element("copyrightbottom").Value),
					HorizontalOrientation = (HorizontalTextOrientation)Enum.Parse(typeof(HorizontalTextOrientation), formatting.Element("textorientation").Element("horizontal").Value, true),
					VerticalOrientation = (VerticalTextOrientation)Enum.Parse(typeof(VerticalTextOrientation), formatting.Element("textorientation").Element("vertical").Value, true),
					BorderBottom = int.Parse(formatting.Element("borders").Element("mainbottom").Value),
					BorderTop = int.Parse(formatting.Element("borders").Element("maintop").Value),
					BorderLeft = int.Parse(formatting.Element("borders").Element("mainleft").Value),
					BorderRight = int.Parse(formatting.Element("borders").Element("mainright").Value),
					IsOutlineEnabled = bool.Parse(formatting.Element("font").Element("outline").Element("enabled").Value),
					OutlineColor = ParsePowerpraiseColor(formatting.Element("font").Element("outline").Element("color").Value),
					IsShadowEnabled = bool.Parse(formatting.Element("font").Element("shadow").Element("enabled").Value),
					ShadowColor = ParsePowerpraiseColor(formatting.Element("font").Element("shadow").Element("color").Value),
					ShadowDirection = int.Parse(formatting.Element("font").Element("shadow").Element("direction").Value),
					TranslationPosition = formatting.Element("textorientation").Element("transpos") != null ?
						(TranslationPosition)Enum.Parse(typeof(TranslationPosition), formatting.Element("textorientation").Element("transpos").Value, true) : TranslationPosition.Inline,
					CopyrightDisplayPosition = (MetadataDisplayPosition)Enum.Parse(typeof(MetadataDisplayPosition), root.Element("information").Element("copyright").Element("position").Value, true),
					SourceDisplayPosition = (MetadataDisplayPosition)Enum.Parse(typeof(MetadataDisplayPosition), root.Element("information").Element("source").Element("position").Value, true)
				};

				if (root.Element("general").Element("category") != null)
					this.Category = root.Element("general").Element("category").Value;
				if (root.Element("general").Element("language") != null)
					this.Language = root.Element("general").Element("language").Value;
				if (root.Element("general").Element("comment") != null)
					Comment = root.Element("general").Element("comment").Value;
				else
					Comment = String.Empty;

				this.Parts = (from part in root.Element("songtext").Elements("part")
							  select new SongPart
							  {
								  Name = part.Attribute("caption").Value,
								  Slides = (from slide in part.Elements("slide")
											select new SongSlide
											{
												Text = string.Join("\n", slide.Elements("line").Select(line => line.Value).ToArray())/*.Trim()*/,
												Translation = string.Join("\n", slide.Elements("translation").Select(line => line.Value).ToArray())/*.Trim()*/,
												BackgroundIndex = slide.Attribute("backgroundnr") != null ? int.Parse(slide.Attribute("backgroundnr").Value) : 0,
												Size = slide.Attribute("mainsize") != null ? int.Parse(slide.Attribute("mainsize").Value) : Formatting.MainText.Size
											}).ToList()
							  }).ToList();
				this.Order = (from item in root.Element("order").Elements("item") select item.Value).ToList();

				this.Copyright = string.Join("\n", root.Element("information").Element("copyright").Element("text").Elements("line").Select(line => line.Value).ToArray());
				this.Sources = new List<SongSource>
				{
					SongSource.Parse(string.Join("\n", root.Element("information").Element("source").Element("text").Elements("line").Select(line => line.Value).ToArray()))
				};
			}
		}

		public void SavePowerpraise(string fileName)
		{
			XDocument doc = new XDocument(new XDeclaration("1.0","ISO-8859-1","yes"));
			XElement root = new XElement("ppl", new XAttribute("version", "3.0"),
				new XElement("general",
					new XElement("title", this.SongTitle),
					new XElement("category",this.Category),
					new XElement("language", this.Language),
					String.IsNullOrEmpty(this.Comment) ? null : new XElement("comment", this.Comment)),
				new XElement("songtext",
					from part in this.Parts select new XElement("part",
						new XAttribute("caption", part.Name),
						from slide in part.Slides select new XElement("slide",
							new XAttribute("mainsize", slide.Size),
							new XAttribute("backgroundnr", slide.BackgroundIndex),
							!String.IsNullOrEmpty(slide.Text) ?
								from line in slide.Text.Split('\n') select new XElement("line", RemoveLineBreaks(line)) : null,
							!String.IsNullOrEmpty(slide.Translation) ?
								from translationline in slide.Translation.Split('\n') select new XElement("translation", RemoveLineBreaks(translationline)) : null
						)
					)
				),
				new XElement("order",
					from item in this.Order select new XElement("item", item)
				),
				new XElement("information",
					new XElement("copyright",
						new XElement("position", this.Formatting.CopyrightDisplayPosition.ToString().ToLower()),
						new XElement("text",
							!String.IsNullOrEmpty(this.Copyright) ?
								from line in this.Copyright.Split('\n') select new XElement("line", line) : null
						)
					),
					new XElement("source",
						new XElement("position", this.Formatting.SourceDisplayPosition.ToString().ToLower()),
						new XElement("text",
							this.Sources.Count > 0 && !String.IsNullOrEmpty(this.Sources[0].ToString()) ?
								new XElement("line", this.Sources[0].ToString()) : null
						)
					)
				),
				new XElement("formatting",
					new XElement("font",
						SavePowerpraiseTextFormatting(this.Formatting.MainText, "maintext"),
						SavePowerpraiseTextFormatting(this.Formatting.TranslationText, "translationtext"),
						SavePowerpraiseTextFormatting(this.Formatting.CopyrightText, "copyrighttext"),
						SavePowerpraiseTextFormatting(this.Formatting.SourceText, "sourcetext"),
						new XElement("outline",
							new XElement("enabled", this.Formatting.IsOutlineEnabled.ToString().ToLower()),
							new XElement("color", CreatePowerpraiseColor(this.Formatting.OutlineColor))
						),
						new XElement("shadow",
							new XElement("enabled", this.Formatting.IsShadowEnabled.ToString().ToLower()),
							new XElement("color", CreatePowerpraiseColor(this.Formatting.ShadowColor)),
							new XElement("direction", this.Formatting.ShadowDirection)
						)
					),
					new XElement("background",
						from bg in this.Backgrounds select new XElement("file", SavePowerpraiseBackground(bg))
					),
					new XElement("linespacing",
						new XElement("main", this.Formatting.TextLineSpacing),
						new XElement("translation", this.Formatting.TranslationLineSpacing)
					),
					new XElement("textorientation",
						new XElement("horizontal", this.Formatting.HorizontalOrientation.ToString().ToLower()),
						new XElement("vertical", this.Formatting.VerticalOrientation.ToString().ToLower()),
						new XElement("transpos", this.Formatting.TranslationPosition.ToString().ToLower())
					),
					new XElement("borders",
						new XElement("mainleft", this.Formatting.BorderLeft),
						new XElement("maintop", this.Formatting.BorderTop),
						new XElement("mainright", this.Formatting.BorderRight),
						new XElement("mainbottom", this.Formatting.BorderBottom),
						new XElement("copyrightbottom", this.Formatting.CopyrightBorderBottom),
						new XElement("sourcetop", this.Formatting.SourceBorderTop),
						new XElement("sourceright", this.Formatting.SourceBorderRight)
					)
				)
			);
			doc.Add(new XComment("This file was written using Words"));
			doc.Add(root);

			StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("iso-8859-1"));
			doc.Save(writer);
			writer.Close();
		}

		private string RemoveLineBreaks(string input)
		{
			return input.Replace("\n", "").Replace("\r", "");
		}

		private static XElement SavePowerpraiseTextFormatting(SongTextFormatting formatting, string elementName)
		{
			return new XElement(elementName,
				new XElement("name", formatting.Name),
				new XElement("size", formatting.Size),
				new XElement("bold", formatting.Bold.ToString().ToLower()),
				new XElement("italic", formatting.Italic.ToString().ToLower()),
				new XElement("color", CreatePowerpraiseColor(formatting.Color)),
				new XElement("outline", formatting.Outline),
				new XElement("shadow", formatting.Shadow)
			);
		}

		private static SongTextFormatting LoadPowerpraiseTextFormatting(XElement element)
		{
			return new SongTextFormatting
			{
				Name = element.Element("name").Value,
				Size = int.Parse(element.Element("size").Value),
				Bold = bool.Parse(element.Element("bold").Value),
				Italic = bool.Parse(element.Element("italic").Value),
				Color = ParsePowerpraiseColor(element.Element("color").Value),
				Outline = int.Parse(element.Element("outline").Value),
				Shadow = int.Parse(element.Element("shadow").Value)
			};
		}

		private static string SavePowerpraiseBackground(SongBackground background)
		{
			if (background.IsImage)
				return background.ImagePath;
			else
				return CreatePowerpraiseColor(background.Color).ToString();
		}

		private static SongBackground LoadPowerpraiseBackground(string background)
		{
			var bg = new SongBackground();
			if (background == "none")
			{
				bg.Color = Color.Black;
			}
			else if (Regex.IsMatch(background, @"^\d{1,8}$"))
			{
				bg.Color = ParsePowerpraiseColor(background);
			}
			else
			{
				bg.ImagePath = background;
			}
			return bg;
		}

		private static int CreatePowerpraiseColor(Color color)
		{
			return color.R | (color.G << 8) | (color.B << 16);
		}

		private static Color ParsePowerpraiseColor(string color)
		{
			int col = int.Parse(color);
			return Color.FromArgb(col & 255, (col >> 8) & 255, (col >> 16) & 255);
		}

		#endregion

		#region SongBeamer compatibilty
		private void LoadSongBeamer(FileInfo file)
		{
			if (String.IsNullOrEmpty(Template))
				throw new InvalidOperationException("Template file not set.");

			var fi = new FileInfo(Template);
			if (!fi.Exists)
				throw new InvalidOperationException("Template file does not exist.");

			LoadPowerpraise(new FileInfo(Template), false);

			this.Order.Clear();
			this.Parts.Clear();

			using (StreamReader reader = new StreamReader(file.FullName, System.Text.Encoding.Default, true))
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
							PreProcessSongBeamerProperties(properties, out langcount); // langcount > 2 is not supported (text will be ignored)
							currentPart = new SongPart() { Name = FindUnusedPartName() };
						}
					}
					else
					{
						if (line == "---")
						{
							currentPart.Slides.Add(new SongSlide { Size = Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
							currentText = null;
							Parts.Add(currentPart);
							currentPart = new SongPart { Name = FindUnusedPartName() };
							linenum = 0;
						}
						else if (line == "--")
						{
							currentPart.Slides.Add(new SongSlide { Size = Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
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

				currentPart.Slides.Add(new SongSlide { Size = Formatting.MainText.Size, Text = currentText, Translation = currentTrans });
				Parts.Add(currentPart);

				PostProcessSongBeamerProperties(properties);
			}
		}

		private string FindUnusedPartName()
		{
			int i = 1;
			while(FindPartByName(i.ToString()) != null)
			{
				i++;
			}
			return i.ToString();
		}

		private bool IsSongBeamerPartName(string value)
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

		private void PreProcessSongBeamerProperties(Dictionary<string, string> properties, out int langcount)
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
				SongTitle = properties["title"];

			if (properties.ContainsKey("(c)")) // copyright information 	#(c)=1999 Hillsong Music, Australia / Kingsway’s Thankyou Music
				Copyright = properties["(c)"]; 

			if (properties.ContainsKey("categories")) // categories the song is about 	#Categories=grace, worship
				Category = properties["categories"]; 

			if (properties.ContainsKey("comment")) // arbitrary comment 	#Comment=correct slide 4!!!
				Comment = properties["comment"];

			if (properties.ContainsKey("songbook")) // also written: SongBook 	#Songbook=Come to Worship 1+2 / 136, In Love With Jesus 2 / 86
				Sources = (from s in properties["songbook"].Split(',') select SongSource.Parse(s)).ToList();

			if (properties.ContainsKey("langcount")) // number of translations in song file 	#LangCount=1
				langcount = int.Parse(properties["langcount"]);
			else
				langcount = 1;

			if (properties.ContainsKey("lang")) // language of the song 	#Lang=E
				Language = properties["lang"]; // TODO: what format is this in?
		}

		private void PostProcessSongBeamerProperties(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("verseorder"))
			{
			    Order = properties["verseorder"].Split(',').ToList();
			}
			else
			{
				foreach (SongPart part in Parts)
				{
					Order.Add(part.Name);
				}
			}
		}
		#endregion
	}
}