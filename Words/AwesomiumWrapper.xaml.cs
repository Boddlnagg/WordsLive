using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Awesomium.Core;
using System.Windows;
using Words.Presentation;
using System.Windows.Media.Animation;
using System;
using Awesomium.Windows.Controls;

namespace Words
{
	/// <summary>
	/// Interaktionslogik für AwesomiumWrapper.xaml
	/// </summary>
	public partial class AwesomiumWrapper : UserControl
	{
		private WebView webView;
		private WebControl webControl;

		private Image frontImage;
		private Image backImage;

		public IWebViewJavaScript Web
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
		private bool swapBitmaps;
		private Int32Rect rect;

		public AwesomiumWrapper()
		{
			InitializeComponent();
		}

		public void Load(bool manualUpdate, PresentationArea area)
		{
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
				wb1 = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
				wb2 = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
				rect = new Int32Rect(0, 0, w, h);

				area.WindowSizeChanged += new System.EventHandler(area_WindowSizeChanged);
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

		public void RenderWebView()
		{
			if (webView == null)
				throw new InvalidOperationException("Manual updating is disabled for this presentation.");

			var buf = webView.Render();
			if (swapBitmaps)
				wb1.WritePixels(rect, buf.Buffer, (int)(buf.Rowspan * buf.Height), buf.Rowspan, 0, 0);
			else
				wb2.WritePixels(rect, buf.Buffer, (int)(buf.Rowspan * buf.Height), buf.Rowspan, 0, 0);
		}

		public void UpdateForeground()
		{
			if (webView == null)
				throw new InvalidOperationException("Manual updating is disabled for this presentation.");

			if (frontImage == null)
			{
				frontImage = new Image { Stretch = Stretch.Fill };
				backImage = new Image { Stretch = Stretch.Fill };
				BackgroundGrid.Children.Add(backImage);
				ForegroundGrid.Children.Add(frontImage);
			}

			backImage.Source = frontImage.Source;
			frontImage.Source = swapBitmaps ? wb1 : wb2;
			swapBitmaps = !swapBitmaps;
		}
	}
}
