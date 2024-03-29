﻿/*
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

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using WordsLive.Core.Songs;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class SongSettingsWindow : Window, IDataErrorInfo
	{
		private SongFormatting formatting;

		public SongFormatting Formatting
		{
			get
			{
				return formatting;
			}
		}

		public int BorderTop
		{
			get
			{
				return formatting.BorderTop;
			}
			set
			{
				formatting.BorderTop = value;
			}
		}

		public int BorderBottom
		{
			get
			{
				return formatting.BorderBottom;
			}
			set
			{
				formatting.BorderBottom = value;
			}
		}

		public int BorderLeft
		{
			get
			{
				return formatting.BorderLeft;
			}
			set
			{
				formatting.BorderLeft = value;
			}
		}

		public int BorderRight
		{
			get
			{
				return formatting.BorderRight;
			}
			set
			{
				formatting.BorderRight = value;
			}
		}

		public int TextLineSpacing
		{
			get
			{
				return formatting.TextLineSpacing;
			}
			set
			{
				formatting.TextLineSpacing = value;
			}
		}

		public int TranslationLineSpacing
		{
			get
			{
				return formatting.TranslationLineSpacing;
			}
			set
			{
				formatting.TranslationLineSpacing = value;
			}
		}

		public CombinedTextOrientation TextOrientation
		{
			get
			{
				return (CombinedTextOrientation)((int)formatting.HorizontalOrientation + (int)formatting.VerticalOrientation * 3);
			}
			set
			{
				formatting.HorizontalOrientation = (HorizontalTextOrientation)((int)value % 3);
				formatting.VerticalOrientation = (VerticalTextOrientation)((int)value / 3);
			}
		}

		public TranslationPosition TranslationPosition
		{
			get
			{
				return formatting.TranslationPosition;
			}
			set
			{
				formatting.TranslationPosition = value;
			}
		}

		public MetadataDisplayPosition CopyrightDisplayPosition
		{
			get
			{
				return formatting.CopyrightDisplayPosition;
			}
			set
			{
				formatting.CopyrightDisplayPosition = value;
			}
		}

		public MetadataDisplayPosition SourceDisplayPosition
		{
			get
			{
				return formatting.SourceDisplayPosition;
			}
			set
			{
				formatting.SourceDisplayPosition = value;
			}
		}

		public int CopyrightBorderBottom
		{
			get
			{
				return formatting.CopyrightBorderBottom;
			}
			set
			{
				formatting.CopyrightBorderBottom = value;
			}
		}

		public int SourceBorderTop
		{
			get
			{
				return formatting.SourceBorderTop;
			}
			set
			{
				formatting.SourceBorderTop = value;
			}
		}

		public int SourceBorderRight
		{
			get
			{
				return formatting.SourceBorderRight;
			}
			set
			{
				formatting.SourceBorderRight = value;
			}
		}

		public SongTextFormatting CopyrightText
		{
			get
			{
				return formatting.CopyrightText;
			}
			set
			{
				formatting.CopyrightText = value;
			}
		}

		public SongTextFormatting SourceText
		{
			get
			{
				return formatting.SourceText;
			}
			set
			{
				formatting.SourceText = value;
			}
		}

		public SongTextFormatting TranslationText
		{
			get
			{
				return formatting.TranslationText;
			}
			set
			{
				formatting.TranslationText = value;
			}
		}

		public SongTextFormatting MainText
		{
			get
			{
				return formatting.MainText;
			}
			set
			{
				formatting.MainText = value;
			}
		}

		public bool SingleFontSize
		{
			get
			{
				return formatting.SingleFontSize;
			}
			set
			{
				formatting.SingleFontSize = value;
			}
		}

		public bool IsOutlineEnabled
		{
			get
			{
				return formatting.IsOutlineEnabled;
			}
			set
			{
				formatting.IsOutlineEnabled = value;
			}
		}

		public Color OutlineColor
		{
			get
			{
				return Color.FromRgb(formatting.OutlineColor.R, formatting.OutlineColor.G, formatting.OutlineColor.B);
			}
			set
			{
				formatting.OutlineColor = System.Drawing.Color.FromArgb(value.R, value.G, value.B);
			}
		}

		public bool IsShadowEnabled
		{
			get
			{
				return formatting.IsShadowEnabled;
			}
			set
			{
				formatting.IsShadowEnabled = value;
			}
		}

		public int ShadowDirection
		{
			get
			{
				return formatting.ShadowDirection;
			}
			set
			{
				formatting.ShadowDirection = value;
			}
		}

		public Color ShadowColor
		{
			get
			{
				return Color.FromRgb(formatting.ShadowColor.R, formatting.ShadowColor.G, formatting.ShadowColor.B);
			}
			set
			{
				formatting.ShadowColor = System.Drawing.Color.FromArgb(value.R, value.G, value.B);
			}
		}


		public SongSettingsWindow(SongFormatting formatting)
		{
			InitializeComponent();
			this.formatting = formatting;
			this.DataContext = this;
		}

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			if (this.IsValid())
			{
				DialogResult = true;
				this.Close();
			}
		}

		public string Error
		{
			get { return null; }
		}

		public string this[string columnName]
		{
			get
			{
				switch(columnName)
				{
					case "BorderTop":
						if (BorderTop < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "BorderBottom":
						if (BorderBottom < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "BorderLeft":
						if (BorderLeft < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "BorderRight":
						if (BorderRight < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "TextLineSpacing":
						if (TextLineSpacing < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "TranslationLineSpacing":
						if (TranslationLineSpacing < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "TranslationPosition":
						if (TranslationPosition == Core.Songs.TranslationPosition.Block)
							return "Diese Option wird zurzeit noch nicht unterstützt";
						break;
				}

				return null;
			}
		}
	}

	public enum CombinedTextOrientation
	{
		TopLeft,
		TopCenter,
		TopRight,
		CenterLeft,
		CenterCenter,
		CenterRight,
		BottomLeft,
		BottomCenter,
		BottomRight
	}
}
