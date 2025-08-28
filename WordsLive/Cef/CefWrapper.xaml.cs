/*
 * WordsLive - worship projection software
 * Copyright (c) 2020 Patrick Reisert
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CefSharp;
using WordsLive.Presentation;

namespace WordsLive.Cef
{
	public partial class CefWrapper : UserControl
	{
		private CefSharp.Wpf.ChromiumWebBrowser webControl;
		private CefSharp.OffScreen.ChromiumWebBrowser offscreenControl;
		private PresentationArea area;

		private Image frontImage;
		private Image backImage;

		public IWebBrowser Web
		{
			get
			{
				if (offscreenControl != null)
					return offscreenControl;
				else
					return webControl;
			}
		}

		public CefWrapper()
		{
			InitializeComponent();
		}

		public void Load(bool manualUpdate, PresentationArea area)
		{
			this.area = area;

			if (!manualUpdate)
			{
				webControl = new CefSharp.Wpf.ChromiumWebBrowser();
				ForegroundGrid.Children.Add(webControl);
			}
			else
			{
				offscreenControl = new CefSharp.OffScreen.ChromiumWebBrowser("about:blank", new BrowserSettings { BackgroundColor = CefSharp.Cef.ColorSetARGB(0, 0, 0, 0) } );
				offscreenControl.Size = this.area.WindowSize;
				area.WindowSizeChanged += area_WindowSizeChanged;
			}
		}

		void area_WindowSizeChanged(object sender, System.EventArgs e)
		{
			offscreenControl.Size = ((PresentationArea)sender).WindowSize;
		}

		private static BitmapSource CreateBitmapSourceFromGdiBitmap(System.Drawing.Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException("bitmap");

			var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

			var bitmapData = bitmap.LockBits(
				rect,
				System.Drawing.Imaging.ImageLockMode.ReadWrite,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			try
			{
				var size = (rect.Width * rect.Height) * 4;

				return BitmapSource.Create(
					bitmap.Width,
					bitmap.Height,
					bitmap.HorizontalResolution,
					bitmap.VerticalResolution,
					PixelFormats.Bgra32,
					null,
					bitmapData.Scan0,
					size,
					bitmapData.Stride);
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}
		}

		public void UpdateForeground()
		{
			if (offscreenControl == null)
				throw new InvalidOperationException("Manual updating is disabled for this presentation.");

			if (frontImage == null)
			{
				frontImage = new Image { Stretch = Stretch.Fill };
				backImage = new Image { Stretch = Stretch.Fill };
				BackgroundGrid.Children.Add(backImage);
				ForegroundGrid.Children.Add(frontImage);
			}

			backImage.Source = frontImage.Source;
			using (var bitmap = offscreenControl.ScreenshotOrNull())
			{
				frontImage.Source = CreateBitmapSourceFromGdiBitmap(bitmap);
			}
		}

		public void Close()
		{
			if (area != null)
			{
				area.WindowSizeChanged -= area_WindowSizeChanged;
			}

			if (Web != null)
			{
				Web.Dispose();
			}
		}
	}
}
