using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Words.Core.Songs
{
	public class SongFormatting : ICloneable
	{
		public SongTextFormatting MainText { get; set; }
		public SongTextFormatting TranslationText { get; set; }
		public SongTextFormatting SourceText { get; set; }
		public SongTextFormatting CopyrightText { get; set; }
		public int TextLineSpacing { get; set; }
		public int TranslationLineSpacing { get; set; }
		public int CopyrightBorderBottom { get; set; }
		public int SourceBorderTop { get; set; }
		public int SourceBorderRight { get; set; }
		public HorizontalTextOrientation HorizontalOrientation { get; set; }
		public VerticalTextOrientation VerticalOrientation { get; set; }
		public int BorderLeft { get; set; }
		public int BorderRight { get; set; }
		public int BorderTop { get; set; }
		public int BorderBottom { get; set; }
		public bool IsOutlineEnabled { get; set; }
		public Color OutlineColor { get; set; }
		public bool IsShadowEnabled { get; set; }
		public Color ShadowColor { get; set; }
		public int ShadowDirection { get; set; }
		public TranslationPosition TranslationPosition { get; set; }
		public MetadataDisplayPosition CopyrightDisplayPosition { get; set; }
		public MetadataDisplayPosition SourceDisplayPosition { get; set; }

		public object Clone()
		{
			SongFormatting clone = (SongFormatting)this.MemberwiseClone();
			clone.MainText = (SongTextFormatting)MainText.Clone();
			clone.TranslationText = (SongTextFormatting)TranslationText.Clone();
			clone.SourceText = (SongTextFormatting)SourceText.Clone();
			clone.CopyrightText = (SongTextFormatting)CopyrightText.Clone();
			return clone;
		}
	}

	public class SongTextFormatting : ICloneable
	{
		public int Size { get; set; }
		public string Name { get; set; }
		public bool Bold { get; set; }
		public bool Italic { get; set; }
		public Color Color { get; set; }
		public int Outline { get; set; }
		public int Shadow { get; set; }

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}

	public enum HorizontalTextOrientation
	{ 
		Left = 0,
		Center = 1,
		Right = 2
	}

	public enum VerticalTextOrientation
	{ 
		Top = 0,
		Center = 1,
		Bottom = 2
	}

	public enum MetadataDisplayPosition
	{ 
		None,
		FirstSlide,
		LastSlide,
		AllSlides
	}

	public enum TranslationPosition
	{ 
		Inline,
		Block
	}
}
