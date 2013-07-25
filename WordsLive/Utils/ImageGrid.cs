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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WordsLive.Utils
{
	public class ImageGrid : Grid
	{
		public static DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageGrid), new PropertyMetadata(null, SourcePropertyChanged));
		public static DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageGrid), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure, StretchPropertyChanged));

		public ImageSource Source
		{
			get
			{
				return (ImageSource)GetValue(SourceProperty);
			}
			set
			{
				SetValue(SourceProperty, value);
			}
		}

		public Stretch Stretch
		{
			get
			{
				return (Stretch)GetValue(StretchProperty);
			}
			set
			{
				SetValue(StretchProperty, value);
			}
		}

		private Image image;

		public ImageGrid()
		{
			image = new Image();
			this.Children.Add(image);
		}

		private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sender = (ImageGrid)d;
			sender.image.Source = (ImageSource)e.NewValue;
		}

		private static void StretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sender = (ImageGrid)d;
			sender.image.Stretch = (Stretch)e.NewValue;
		}
	}
}
