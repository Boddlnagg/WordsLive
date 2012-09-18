using System.ComponentModel;
using System.Windows;
using WordsLive.Core.Songs;
using WordsLive.Utils;
using System.Windows.Media;

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
				return Formatting.BorderTop;
			}
			set
			{
				Formatting.BorderTop = value;
			}
		}

		public int BorderBottom
		{
			get
			{
				return Formatting.BorderBottom;
			}
			set
			{
				Formatting.BorderBottom = value;
			}
		}

		public int BorderLeft
		{
			get
			{
				return Formatting.BorderLeft;
			}
			set
			{
				Formatting.BorderLeft = value;
			}
		}

		public int BorderRight
		{
			get
			{
				return Formatting.BorderRight;
			}
			set
			{
				Formatting.BorderRight = value;
			}
		}

		public int TextLineSpacing
		{
			get
			{
				return Formatting.TextLineSpacing;
			}
			set
			{
				Formatting.TextLineSpacing = value;
			}
		}

		public int TranslationLineSpacing
		{
			get
			{
				return Formatting.TranslationLineSpacing;
			}
			set
			{
				Formatting.TranslationLineSpacing = value;
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
				return Formatting.TranslationPosition;
			}
			set
			{
				Formatting.TranslationPosition = value;
			}
		}

		public MetadataDisplayPosition CopyrightDisplayPosition
		{
			get
			{
				return Formatting.CopyrightDisplayPosition;
			}
			set
			{
				Formatting.CopyrightDisplayPosition = value;
			}
		}

		public MetadataDisplayPosition SourceDisplayPosition
		{
			get
			{
				return Formatting.SourceDisplayPosition;
			}
			set
			{
				Formatting.SourceDisplayPosition = value;
			}
		}

		public int CopyrightBorderBottom
		{
			get
			{
				return Formatting.CopyrightBorderBottom;
			}
			set
			{
				Formatting.CopyrightBorderBottom = value;
			}
		}

		public int SourceBorderTop
		{
			get
			{
				return Formatting.SourceBorderTop;
			}
			set
			{
				Formatting.SourceBorderTop = value;
			}
		}

		public int SourceBorderRight
		{
			get
			{
				return Formatting.SourceBorderRight;
			}
			set
			{
				Formatting.SourceBorderRight = value;
			}
		}

		public SongTextFormatting CopyrightText
		{
			get
			{
				return Formatting.CopyrightText;
			}
			set
			{
				Formatting.CopyrightText = value;
			}
		}

		public SongTextFormatting SourceText
		{
			get
			{
				return Formatting.SourceText;
			}
			set
			{
				Formatting.SourceText = value;
			}
		}

		public SongTextFormatting TranslationText
		{
			get
			{
				return Formatting.TranslationText;
			}
			set
			{
				Formatting.TranslationText = value;
			}
		}

		public SongTextFormatting MainText
		{
			get
			{
				return Formatting.MainText;
			}
			set
			{
				Formatting.MainText = value;
			}
		}

		public bool IsOutlineEnabled
		{
			get
			{
				return Formatting.IsOutlineEnabled;
			}
			set
			{
				Formatting.IsOutlineEnabled = value;
			}
		}

		public Color OutlineColor
		{
			get
			{
				return Color.FromRgb(Formatting.OutlineColor.R, Formatting.OutlineColor.G, Formatting.OutlineColor.B);
			}
			set
			{
				Formatting.OutlineColor = System.Drawing.Color.FromArgb(value.R, value.G, value.B);
			}
		}

		public bool IsShadowEnabled
		{
			get
			{
				return Formatting.IsShadowEnabled;
			}
			set
			{
				Formatting.IsShadowEnabled = value;
			}
		}

		public int ShadowDirection
		{
			get
			{
				return Formatting.ShadowDirection;
			}
			set
			{
				Formatting.ShadowDirection = value;
			}
		}

		public Color ShadowColor
		{
			get
			{
				return Color.FromRgb(Formatting.ShadowColor.R, Formatting.ShadowColor.G, Formatting.ShadowColor.B);
			}
			set
			{
				Formatting.ShadowColor = System.Drawing.Color.FromArgb(value.R, value.G, value.B);
			}
		}


		public SongSettingsWindow(SongFormatting formatting)
		{
			InitializeComponent();
			this.formatting = (SongFormatting)formatting.Clone();
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
