/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using Awesomium.Core;
using Awesomium.Windows.Controls;
using WordsLive.Presentation;

namespace WordsLive.Awesomium
{
	/// <summary>
	/// Interaktionslogik für AwesomiumWrapper.xaml
	/// </summary>
	public partial class AwesomiumWrapper : UserControl
	{
		private WebView webView;
		private WebControl webControl;
		private PresentationArea area;

		private Image frontImage;
		private Image backImage;

		public IWebView Web
		{
			get
			{
				if (webView != null)
					return webView;
				else
					return webControl;
			}
		}

		private WriteableBitmap wb1, wb2;
		private Int32Rect rect;

		public AwesomiumWrapper()
		{
			InitializeComponent();
		}

		public void Load(bool manualUpdate, PresentationArea area)
		{
			this.area = area;

			if (!manualUpdate)
			{
				webControl = new WebControl();
				ForegroundGrid.Children.Add(webControl);
			}
			else
			{
				int w = area.WindowSize.Width;
				int h = area.WindowSize.Height;
				webView = WebCore.CreateWebView(w, h);
				webView.Surface = new ImageSurface(null);
				wb1 = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
				wb2 = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
				rect = new Int32Rect(0, 0, w, h);

				area.WindowSizeChanged += area_WindowSizeChanged;
			}
		}

		void area_WindowSizeChanged(object sender, System.EventArgs e)
		{
			int w = ((PresentationArea)sender).WindowSize.Width;
			int h = ((PresentationArea)sender).WindowSize.Height;

			webView.Resize(w, h);

			wb1 = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
			wb2 = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
			rect = new Int32Rect(0, 0, w, h);
		}

		public void UpdateForeground()
		{
			if (webView == null)
				throw new InvalidOperationException("Manual updating is disabled for this presentation.");

			var surf = webView.Surface as ImageSurface;

			if (frontImage == null)
			{
				frontImage = new Image { Stretch = Stretch.Fill };
				backImage = new Image { Stretch = Stretch.Fill };
				BackgroundGrid.Children.Add(backImage);
				ForegroundGrid.Children.Add(frontImage);
			}

			backImage.Source = frontImage.Source;
			frontImage.Source = surf.Image.Clone();
		}

		public void Close()
		{
			if (area != null)
			{
				area.WindowSizeChanged -= area_WindowSizeChanged;
			}
		}
	}
}
