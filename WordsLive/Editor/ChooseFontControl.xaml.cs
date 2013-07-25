/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
