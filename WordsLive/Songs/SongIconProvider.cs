using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Words.Core;
using Words.Core.Songs;
using Words.MediaOrderList;
using System.IO;

namespace Words.Songs
{
	[TargetMedia(typeof(Song))]
	public class SongIconProvider : IconProvider
	{
		public SongIconProvider(Song data) : base(data)
		{ }

		protected override ImageSource CreateIcon()
		{
			Song s = (Song)this.Data; // this only contains title and backgrounds
			if (s.Backgrounds[0].IsImage)
			{
				try
				{
					var img = new BitmapImage();
					img.BeginInit();
					img.UriSource = new Uri(Path.Combine(MediaManager.BackgroundsDirectory, s.Backgrounds[0].ImagePath));
					img.DecodePixelWidth = 22;
					img.EndInit();
					return img;
				}
				catch
				{
					return CreateColorImage(Brushes.Black);
				}
			}
			else
			{
				// create an empty image with the given background color
				var c = s.Backgrounds[0].Color;
				var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
				return CreateColorImage(brush);
			}
		}

		private ImageSource CreateColorImage(Brush brush)
		{
			RenderTargetBitmap rtb = new RenderTargetBitmap(22, 16, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
			var rect = new System.Windows.Shapes.Rectangle { Width = 22, Height = 16, Fill = brush };
			rect.Arrange(new System.Windows.Rect(0, 0, rect.Width, rect.Height));
			rtb.Render(rect);
			return rtb;
		}
	}
}
