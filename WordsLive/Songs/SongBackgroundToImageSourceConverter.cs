using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using WordsLive.Core.Songs;
using System.Windows.Media.Imaging;
using System.IO;
using WordsLive.Core;

namespace WordsLive.Songs
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
			if (bg.Type == SongBackgroundType.Image)
			{
				try
				{
					var img = new BitmapImage();
					img.BeginInit();
					img.UriSource = new Uri(Path.Combine(MediaManager.BackgroundsDirectory, bg.FilePath)); // TODO: use BackgroundDataProvider
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
