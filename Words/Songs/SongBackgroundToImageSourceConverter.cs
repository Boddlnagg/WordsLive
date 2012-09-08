using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Words.Core.Songs;
using System.Windows.Media.Imaging;
using System.IO;
using Words.Core;

namespace Words.Songs
{
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
			if (bg.IsImage)
			{
				try
				{
					var img = new BitmapImage();
					img.BeginInit();
					img.UriSource = new Uri(Path.Combine(MediaManager.BackgroundsDirectory, bg.ImagePath));
					if (width > -1)
						img.DecodePixelWidth = width;
					img.EndInit();
					WriteableBitmap writable = new WriteableBitmap(img);
					writable.Freeze();
					return writable;
				}
				catch
				{
					return CreateColorImage(Brushes.Black);
				}
			}
			else
			{
				var c = bg.Color;
				var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
				return CreateColorImage(brush);
			}
		}

		private static ImageSource CreateColorImage(Brush brush)
		{
			RenderTargetBitmap rtb = new RenderTargetBitmap(1, 1, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
			var rect = new System.Windows.Shapes.Rectangle { Width = 1, Height = 1, Fill = brush };
			rect.Arrange(new System.Windows.Rect(0, 0, rect.Width, rect.Height));
			rtb.Render(rect);
			return rtb;
		}
	}
}
