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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// Reads the song data in the Powerpraise XML format (ppl).
	/// </summary>
	public class PowerpraiseSongReader : ISongReader
	{
		/// <summary>
		/// Reads the song data from a stream.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <param name="stream">The stream.</param>
		public void Read(Song song, Stream stream)
		{
			XDocument doc = XDocument.Load(stream);
			XElement root = doc.Element("ppl");
			song.Title = root.Element("general").Element("title").Value;

			var formatting = root.Element("formatting");

			// reset in case it has already been loaded (TODO: move outside of this class to caller)
			song.Backgrounds.Clear(); // this is needed, because the indices must be correct
			song.Sources.Clear();
			song.Parts.Clear();

			var video = root.Element("formatting").Element("background").Attribute("video");
			if (video != null)
			{
				song.Backgrounds.Add(new SongBackground(video.Value, true));
			}
			else
			{
				foreach (var bg in root.Element("formatting").Element("background").Elements("file"))
				{
					song.Backgrounds.Add(ReadBackground(bg.Value));
				}
			}

			song.Formatting = new SongFormatting
			{
				MainText = ReadTextFormatting(formatting.Element("font").Element("maintext")),
				TranslationText = ReadTextFormatting(formatting.Element("font").Element("translationtext")),
				SourceText = ReadTextFormatting(formatting.Element("font").Element("sourcetext")),
				CopyrightText = ReadTextFormatting(formatting.Element("font").Element("copyrighttext")),
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
				OutlineColor = ParseColor(formatting.Element("font").Element("outline").Element("color").Value),
				IsShadowEnabled = bool.Parse(formatting.Element("font").Element("shadow").Element("enabled").Value),
				ShadowColor = ParseColor(formatting.Element("font").Element("shadow").Element("color").Value),
				ShadowDirection = int.Parse(formatting.Element("font").Element("shadow").Element("direction").Value),
				TranslationPosition = formatting.Element("textorientation").Element("transpos") != null ?
					(TranslationPosition)Enum.Parse(typeof(TranslationPosition), formatting.Element("textorientation").Element("transpos").Value, true) : TranslationPosition.Inline,
				CopyrightDisplayPosition = (MetadataDisplayPosition)Enum.Parse(typeof(MetadataDisplayPosition), root.Element("information").Element("copyright").Element("position").Value, true),
				SourceDisplayPosition = (MetadataDisplayPosition)Enum.Parse(typeof(MetadataDisplayPosition), root.Element("information").Element("source").Element("position").Value, true)
			};

			if (root.Element("general").Element("category") != null)
				song.Category = root.Element("general").Element("category").Value;
			if (root.Element("general").Element("language") != null)
				song.Language = root.Element("general").Element("language").Value;
			if (root.Element("general").Element("comment") != null)
				song.Comment = root.Element("general").Element("comment").Value;
			else
				song.Comment = String.Empty;

			foreach (var part in root.Element("songtext").Elements("part"))
			{
				song.Parts.Add(new SongPart(song,
								part.Attribute("caption").Value,
								from slide in part.Elements("slide")
								select new SongSlide(song)
								{
									Text = String.Join("\n", slide.Elements("line").Select(line => line.Value).ToArray())/*.Trim()*/,
									Translation = String.Join("\n", slide.Elements("translation").Select(line => line.Value).ToArray())/*.Trim()*/,
									BackgroundIndex = slide.Attribute("backgroundnr") != null ? int.Parse(slide.Attribute("backgroundnr").Value) : 0,
									Size = slide.Attribute("mainsize") != null ? int.Parse(slide.Attribute("mainsize").Value) : song.Formatting.MainText.Size
								}
							));
			}

			song.SetOrder(from item in root.Element("order").Elements("item") select item.Value);

			song.Copyright = String.Join("\n", root.Element("information").Element("copyright").Element("text").Elements("line").Select(line => line.Value).ToArray());

			song.AddSource(String.Join("\n", root.Element("information").Element("source").Element("text").Elements("line").Select(line => line.Value)));
		}

		/// <summary>
		/// Helper method to read a <see cref="SongTextFormatting"/> object from an XML object.
		/// </summary>
		/// <param name="element">The XML element.</param>
		/// <returns>The loaded formatting object.</returns>
		private static SongTextFormatting ReadTextFormatting(XElement element)
		{
			return new SongTextFormatting
			{
				Name = element.Element("name").Value,
				Size = int.Parse(element.Element("size").Value),
				Bold = bool.Parse(element.Element("bold").Value),
				Italic = bool.Parse(element.Element("italic").Value),
				Color = ParseColor(element.Element("color").Value),
				Outline = int.Parse(element.Element("outline").Value),
				Shadow = int.Parse(element.Element("shadow").Value)
			};
		}

		/// <summary>
		/// Helper method to load a <see cref="SongBackground"/> from XML (either an image path or a color).
		/// </summary>
		/// <param name="background">The string encoding of the background object.</param>
		/// <returns>The loaded background object.</returns>
		private static SongBackground ReadBackground(string background)
		{
			SongBackground bg;
			if (background == "none")
			{
				bg = new SongBackground(Color.Black);
			}
			else if (Regex.IsMatch(background, @"^\d{1,8}$"))
			{
				bg = new SongBackground(ParseColor(background));
			}
			else
			{
				bg = new SongBackground(background, false);
			}
			return bg;
		}

		/// <summary>
		/// Helper method to parse a color encoded in Powerpraise XML.
		/// </summary>
		/// <param name="color">The string containing the encoded color.</param>
		/// <returns>The color.</returns>
		private static Color ParseColor(string color)
		{
			int col = int.Parse(color);
			return Color.FromArgb(col & 255, (col >> 8) & 255, (col >> 16) & 255);
		}
	}
}
