using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WordsLive.Core.Songs;

namespace WordsLive.Editor
{
	public partial class ChooseFontControl : UserControl
	{
		public static readonly DependencyProperty FontProperty = DependencyProperty.Register("Font", typeof(SongTextFormatting), typeof(ChooseFontControl), new PropertyMetadata(null, new PropertyChangedCallback(FontPropertyChanged)));

		public SongTextFormatting Font
		{
			get
			{
				return (SongTextFormatting)GetValue(FontProperty);
			}
			set
			{
				SetValue(FontProperty, value);
			}
		}

		public static void FontPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			var c = (ChooseFontControl)sender;
			var val = (SongTextFormatting)args.NewValue;
			c.FontComboBox.SelectedIndex = Fonts.SystemFontFamilies.ToList().IndexOf(new FontFamily(val.Name));
			c.ColorPicker.SelectedColor = Color.FromRgb(val.Color.R, val.Color.G, val.Color.B);
		}

		public ChooseFontControl()
		{
			InitializeComponent();
			FontComboBox.SelectionChanged += (sender, args) =>
			{
				var font = this.Font;
				font.Name = ((FontFamily)FontComboBox.SelectedItem).Source;
				this.Font = font;
			};
			ColorPicker.SelectedColorChanged += (sender, args) =>
			{
				var col = ColorPicker.SelectedColor;
				var font = this.Font;
				font.Color = System.Drawing.Color.FromArgb(col.R, col.G, col.B);
				this.Font = font;
			};
		}
	}
}
