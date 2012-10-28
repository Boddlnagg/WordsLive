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

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents the formatting options for a song.
	/// </summary>
	public struct SongFormatting
	{
		/// <summary>
		/// Gets or sets the text formatting options for the main text.
		/// </summary>
		public SongTextFormatting MainText;

		/// <summary>
		/// Gets or sets the text formatting options for the translation.
		/// </summary>
		public SongTextFormatting TranslationText;

		/// <summary>
		/// Gets or sets the text formatting options for the source.
		/// </summary>
		public SongTextFormatting SourceText;

		/// <summary>
		/// Gets or sets the text formatting options for the copyright.
		/// </summary>
		public SongTextFormatting CopyrightText;

		/// <summary>
		/// Gets or sets the spacing between two lines of text.
		/// </summary>
		public int TextLineSpacing;

		/// <summary>
		/// Gets or sets the spacing between a line and its translation.
		/// </summary>
		public int TranslationLineSpacing;

		/// <summary>
		/// Gets or sets the margin below the copyright.
		/// </summary>
		public int CopyrightBorderBottom;

		/// <summary>
		/// Gets or sets the margin above the source.
		/// </summary>
		public int SourceBorderTop;

		/// <summary>
		/// Gets or sets the right margin of the source.
		/// </summary>
		public int SourceBorderRight;

		/// <summary>
		/// Gets or sets the horizontal text orientation.
		/// </summary>
		public HorizontalTextOrientation HorizontalOrientation;

		/// <summary>
		/// Gets or sets the vertical text orientation.
		/// </summary>
		public VerticalTextOrientation VerticalOrientation;
		
		/// <summary>
		/// Gets or sets the left text margin.
		/// </summary>
		public int BorderLeft;

		/// <summary>
		/// Gets or sets the right text margin.
		/// </summary>
		public int BorderRight;

		/// <summary>
		/// Gets or sets the top text margin.
		/// </summary>
		public int BorderTop;

		/// <summary>
		/// Gets or sets the bottom text margin.
		/// </summary>
		public int BorderBottom;
		
		/// <summary>
		/// Gets or sets a value indicating whether the text outline is enabled.
		/// </summary>
		public bool IsOutlineEnabled;

		/// <summary>
		/// Gets or sets the color for the outline if <see cref="IsOutlineEnabled"/> is <c>true</c>.
		/// </summary>
		public Color OutlineColor;

		/// <summary>
		/// Gets or sets a value indicating whether the text shadow is enabled.
		/// </summary>
		public bool IsShadowEnabled;

		/// <summary>
		/// Gets or sets the color for the shadow if <see cref="IsShadowEnabled"/> is <c>true</c>.
		/// </summary>
		public Color ShadowColor;

		/// <summary>
		/// Gets or sets the direction of the shadow.
		/// TODO: currently ignored
		/// </summary>
		public int ShadowDirection;

		/// <summary>
		/// Gets or sets the position where the translation will be shown.
		/// </summary>
		public TranslationPosition TranslationPosition;

		/// <summary>
		/// Gets or sets the display position of the copyright information.
		/// </summary>
		public MetadataDisplayPosition CopyrightDisplayPosition;

		/// <summary>
		/// Gets or sets the display position of the source information.
		/// </summary>
		public MetadataDisplayPosition SourceDisplayPosition;
	}

	#region Enums
	
	/// <summary>
	/// Represents the horizontal orientation of a text on a slide.
	/// </summary>
	public enum HorizontalTextOrientation
	{ 
		/// <summary>
		/// Align the text to the left.
		/// </summary>
		Left = 0,

		/// <summary>
		/// Center the text horizontally.
		/// </summary>
		Center = 1,

		/// <summary>
		/// Align the text to the right.
		/// </summary>
		Right = 2
	}

	/// <summary>
	/// Represents the vertical orientation of a text on a slide.
	/// </summary>
	public enum VerticalTextOrientation
	{ 
		/// <summary>
		/// Align the text to the top.
		/// </summary>
		Top = 0,

		/// <summary>
		/// Center the text vertically.
		/// </summary>
		Center = 1,

		/// <summary>
		/// Align the text to the bottom.
		/// </summary>
		Bottom = 2
	}

	/// <summary>
	/// Represents the display options of metadata (copyright and source) in the presentation.
	/// </summary>
	public enum MetadataDisplayPosition
	{ 
		/// <summary>
		/// Don't display the metadata at all.
		/// </summary>
		None,

		/// <summary>
		/// Display the metadata on the first slide.
		/// </summary>
		FirstSlide,

		/// <summary>
		/// Display the metadata on the last slide.
		/// </summary>
		LastSlide,

		/// <summary>
		/// Display the metadata on all slides.
		/// </summary>
		AllSlides
	}

	/// <summary>
	/// Represents the position of the translation in the presentation.
	/// </summary>
	public enum TranslationPosition
	{ 
		/// <summary>
		/// Show the translation inline (each line of text is followed by its translation).
		/// </summary>
		Inline,

		/// <summary>
		/// Show the translation as a block after the text.
		/// </summary>
		Block
	}

	#endregion
}
