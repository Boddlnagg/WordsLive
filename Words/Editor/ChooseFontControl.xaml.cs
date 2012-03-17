using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Words.Core.Songs;
using System.ComponentModel;

namespace Words.Editor
{
	public partial class ChooseFontControl : UserControl
	{
		public static readonly DependencyProperty FontProperty = DependencyProperty.Register("Font", typeof(SongTextFormatting), typeof(ChooseFontControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(FontPropertyChanged)));

		public SongTextFormatting Font
		{
			get
			{
				return GetValue(FontProperty) as SongTextFormatting;
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
				this.Font.Name = ((FontFamily)FontComboBox.SelectedItem).Source;
			};
			ColorPicker.SelectedColorChanged += (sender, args) =>
			{
				var col = ColorPicker.SelectedColor;
				this.Font.Color = System.Drawing.Color.FromArgb(col.R, col.G, col.B);
			};
		}
	}
}
