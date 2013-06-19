using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using WordsLive.Presentation;

namespace WordsLive
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
				AwesomiumManager.Register(webControl);
				ForegroundGrid.Children.Add(webControl);
			}
			else
			{
				int w = area.WindowSize.Width;
				int h = area.WindowSize.Height;
				webView = WebCore.CreateWebView(w, h);
				AwesomiumManager.Register(webView);
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
