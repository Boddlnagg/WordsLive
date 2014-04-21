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
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Drawing;

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// Writes the song in the Powerpraise XML file (ppl version 3.0).
	/// </summary>
	public class PowerpraiseSongWriter : ISongWriter
	{
		/// <summary>
		/// Writes the song data to a stream.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <param name="stream">The stream.</param>
		public void Write(Song song, Stream stream)
		{
			XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
			XElement root = new XElement("ppl", new XAttribute("version", "3.0"),
				new XElement("general",
					new XElement("title", song.Title),
					new XElement("category", song.Category),
					new XElement("language", song.Language),
					String.IsNullOrWhiteSpace(song.TranslationLanguage) ? null : new XElement("translationlanguage", song.TranslationLanguage),
					song.CcliNumber == null ? null : new XElement("ccli", song.CcliNumber),
					String.IsNullOrEmpty(song.Comment) ? null : new XElement("comment", song.Comment)),
				new XElement("songtext",
					from part in song.Parts
					select new XElement("part",
						new XAttribute("caption", part.Name),
						from slide in part.Slides
						select new XElement("slide",
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
					from item in song.Order select new XElement("item", item.Part.Name)
				),
				new XElement("information",
					new XElement("copyright",
						new XElement("position", song.Formatting.CopyrightDisplayPosition.ToString().ToLower()),
						new XElement("text",
							!String.IsNullOrEmpty(song.Copyright) ?
								from line in song.Copyright.Split('\n') select new XElement("line", line) : null
						)
					),
					new XElement("source",
						new XElement("position", song.Formatting.SourceDisplayPosition.ToString().ToLower()),
						new XElement("text",
							song.Sources.Count > 0 && !String.IsNullOrEmpty(song.Sources[0].ToString()) ?
								new XElement("line", song.Sources[0].ToString()) : null
						)
					)
				),
				new XElement("formatting",
					new XElement("font",
						WriteTextFormatting(song.Formatting.MainText, "maintext"),
						WriteTextFormatting(song.Formatting.TranslationText, "translationtext"),
						WriteTextFormatting(song.Formatting.CopyrightText, "copyrighttext"),
						WriteTextFormatting(song.Formatting.SourceText, "sourcetext"),
						new XElement("outline",
							new XElement("enabled", song.Formatting.IsOutlineEnabled.ToString().ToLower()),
							new XElement("color", ComputeColor(song.Formatting.OutlineColor))
						),
						new XElement("shadow",
							new XElement("enabled", song.Formatting.IsShadowEnabled.ToString().ToLower()),
							new XElement("color", ComputeColor(song.Formatting.ShadowColor)),
							new XElement("direction", song.Formatting.ShadowDirection)
						)
					),
					new XElement("background",
						song.VideoBackground != null ?
						new object[] {
							new XAttribute("video", song.VideoBackground.FilePath),
							new XElement("file", "none") // for backwards compatibility
						} :
						(from bg in song.Backgrounds select new XElement("file", SavePowerpraiseBackground(bg))).ToArray()
					),
					new XElement("linespacing",
						new XElement("main", song.Formatting.TextLineSpacing),
						new XElement("translation", song.Formatting.TranslationLineSpacing)
					),
					new XElement("textorientation",
						new XElement("horizontal", song.Formatting.HorizontalOrientation.ToString().ToLower()),
						new XElement("vertical", song.Formatting.VerticalOrientation.ToString().ToLower()),
						new XElement("transpos", song.Formatting.TranslationPosition.ToString().ToLower())
					),
					new XElement("borders",
						new XElement("mainleft", song.Formatting.BorderLeft),
						new XElement("maintop", song.Formatting.BorderTop),
						new XElement("mainright", song.Formatting.BorderRight),
						new XElement("mainbottom", song.Formatting.BorderBottom),
						new XElement("copyrightbottom", song.Formatting.CopyrightBorderBottom),
						new XElement("sourcetop", song.Formatting.SourceBorderTop),
						new XElement("sourceright", song.Formatting.SourceBorderRight)
					)
				)
			);
			doc.Add(new XComment("This file was written using WordsLive"));
			doc.Add(root);

			// Powerpraise saves all songs as ISO-8859-1, but it can read UTF-8 also
			// (except for the € sign, which for some reason corrupts the file when saved in Powerpraise)
			StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
			doc.Save(writer);
			writer.Close();
		}

		/// <summary>
		/// Removes all line break characters (\n and \r).
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>The processed input.</returns>
		private string RemoveLineBreaks(string input)
		{
			return input.Replace("\n", "").Replace("\r", "");
		}

		/// <summary>
		/// Helper method to generate an XML object from a <see cref="SongTextFormatting"/> object.
		/// </summary>
		/// <param name="formatting">The formatting object.</param>
		/// <param name="elementName">The element name to generate.</param>
		/// <returns>The generated XML.</returns>
		private static XElement WriteTextFormatting(SongTextFormatting formatting, string elementName)
		{
			return new XElement(elementName,
				new XElement("name", formatting.Name),
				new XElement("size", formatting.Size),
				new XElement("bold", formatting.Bold.ToString().ToLower()),
				new XElement("italic", formatting.Italic.ToString().ToLower()),
				new XElement("color", ComputeColor(formatting.Color)),
				new XElement("outline", formatting.Outline),
				new XElement("shadow", formatting.Shadow)
			);
		}

		/// <summary>
		/// Helper method to save a <see cref="SongBackground"/> object to XML.
		/// </summary>
		/// <param name="background">The background object.</param>
		/// <returns>The path if the background is an image, otherwise the encoded color.</returns>
		private static string SavePowerpraiseBackground(SongBackground background)
		{
			if (background.IsFile)
				return background.FilePath;
			else
				return ComputeColor(background.Color).ToString();
		}


		/// <summary>
		/// Helper method to compute the color value used in Powerpraise XML.
		/// </summary>
		/// <param name="color">The color to encode.</param>
		/// <returns>The encoded color value.</returns>
		private static int ComputeColor(Color color)
		{
			return color.R | (color.G << 8) | (color.B << 16);
		}
	}
}
