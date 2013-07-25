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

using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordsLive.Core;
using WordsLive.Core.Songs;

namespace WordsLive.Songs
{
	/// <summary>
	/// Converts a SongBackground to an ImageSource either by loading the image or by creating an image from the color.
	/// </summary>
	class SongBackgroundToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var bg = (SongBackground)value;
			int width;
			if (parameter != null && int.TryParse((string)parameter, out width))
				return CreateBackgroundSource(bg, width);
			else
				return CreateBackgroundSource(bg);

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}

		public static ImageSource CreateBackgroundSource(SongBackground bg, int width = -1)
		{
			if (bg.Type == SongBackgroundType.Image)
			{
				try
				{
					Uri uri;
					if (width > -1 && width <= 300)
						uri = DataManager.Backgrounds.GetFile(bg).PreviewUri;
					else
						uri = DataManager.Backgrounds.GetFile(bg).Uri;

					var img = new BitmapImage();
					img.BeginInit();
					img.UriSource = uri;
					if (width > 300)
						img.DecodePixelWidth = width;
					img.EndInit();

					// Without an additional frozen WritableBitmap loading locally is delayed (hangs)
					// For remote URIs this doesn't work however, so we're not using it then
					if (uri.IsFile)
					{
						WriteableBitmap writable = new WriteableBitmap(img);
						writable.Freeze();
						return writable;
					}
					else
					{
						return img;
					}
				}
				catch
				{
					return CreateColorImage(Brushes.Black);
				}
			}
			else if (bg.Type == SongBackgroundType.Color)
			{
				var c = bg.Color;
				var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
				return CreateColorImage(brush);
			}
			else
			{
				return CreateColorImage(Brushes.Black); // TODO: load video preview?
			}
		}

		/// <summary>
		/// Helper method to create a 4:3 solid color image.
		/// </summary>
		/// <param name="brush">The brush to use to fill the image.</param>
		/// <returns>The created image.</returns>
		private static ImageSource CreateColorImage(Brush brush)
		{
			RenderTargetBitmap rtb = new RenderTargetBitmap(4, 3, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
			var rect = new System.Windows.Shapes.Rectangle { Width = 4, Height = 3, Fill = brush };
			rect.Arrange(new System.Windows.Rect(0, 0, rect.Width, rect.Height));
			rtb.Render(rect);
			return rtb;
		}
	}
}
