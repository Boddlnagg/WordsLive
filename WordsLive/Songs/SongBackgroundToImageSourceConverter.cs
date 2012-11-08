using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordsLive.Core.Data;
using WordsLive.Core.Songs;

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
					var uri = DataManager.Backgrounds.GetFile(bg).Uri;
					var img = new BitmapImage();
					img.BeginInit();
					img.UriSource = uri;
					if (width > -1)
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
