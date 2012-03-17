using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using unoidl.com.sun.star.frame;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.awt;
using unoidl.com.sun.star.beans;
using unoidl.com.sun.star.presentation;
using unoidl.com.sun.star.drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

#if FALSE
namespace Words.Slideshow.Impress
{			
	public class ImpressPresentation : SlideshowPresentationBase
	{
		#region P/Invokes
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		/*
		public static IntPtr SetClassLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size > 4)
				return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
			else
				return new IntPtr(SetClassLongPtr32(hWnd, nIndex, unchecked((uint)dwNewLong.ToInt32())));
		}

		[DllImport("user32.dll", EntryPoint="SetClassLong")]
		public static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

		[DllImport("user32.dll", EntryPoint="SetClassLongPtr")]
		public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size > 4)
				return GetClassLongPtr64(hWnd, nIndex);
			else
				return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
		}

		[DllImport("user32.dll", EntryPoint="GetClassLong")]
		public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint="GetClassLongPtr")]
		public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);
		*/
		#endregion

		XPresentation2 presentation;
		XSlideShowController controller;
		XModel document;
		XWindow window;
		SlideShowListener listener = new SlideShowListener();
		IntPtr hWnd;

		List<ImageSource> thumbnails = new List<ImageSource>();

		public void Load(ImpressMedia media)
		{
			// Start LibreOffice and load file
			unoidl.com.sun.star.uno.XComponentContext localContext = uno.util.Bootstrap.bootstrap();
			unoidl.com.sun.star.lang.XMultiServiceFactory multiServiceFactory = (unoidl.com.sun.star.lang.XMultiServiceFactory)localContext.getServiceManager();
			XComponentLoader componentLoader = (XComponentLoader)multiServiceFactory.createInstance("com.sun.star.frame.Desktop");
			XComponent component = componentLoader.loadComponentFromURL(CreateFileUrl(media.File), "_blank", 0, new PropertyValue[] { });

			// Hide the window and set display
			document = (XModel)component;
			window = document.getCurrentController().getFrame().getContainerWindow();
			window.setVisible(false);
			XSystemDependentWindowPeer xWindowPeer = (XSystemDependentWindowPeer)(window);
			var handle = xWindowPeer.getWindowHandle(new byte[] { }, 1);
			presentation = (XPresentation2)((XPresentationSupplier)component).getPresentation();
			
			presentation.setPropertyValue("Display", new uno.Any(GetDisplayIndex())); // TODO (Slideshow.Impress): test this

			//presentation.setPropertyValue("IsTransitionOnClick", new uno.Any(false));

			CreateThumbnails();

			listener.SlideTransitionStarted += (sender, args) =>
			{
				OnSlideIndexChanged();
			};

			Start();

			controller.gotoSlideIndex(0);

			hWnd = FindWindow("SALTMPSUBFRAME", null);
			//hWnd2 = FindWindowEx(hWnd, IntPtr.Zero, "SALOBJECT", null);
			//hWnd3 = FindWindowEx(wnd, IntPtr.Zero, "SALOBJECTCHILD", null);

			//int CS_DROPSHADOW = 0x20000;
			//var bit = GetClassLongPtr(hWnd, -26).ToInt32() | CS_DROPSHADOW;
			//SetClassLong(hWnd, -26, new IntPtr(GetClassLongPtr(hWnd, -26).ToInt32() ^ CS_DROPSHADOW));
			//bit = GetClassLongPtr(hWnd, -26).ToInt32() | CS_DROPSHADOW;

			// Resizing the window works (but a dropshadow remains) and moving doesn't, so we stay fullscreen
			//MoveWindow(hWnd, 0, 0, 800, 600, true);


			ShowWindow(hWnd, 0);

			LoadPreviewProvider();
			
			base.OnLoaded();
		}

		private int GetDisplayIndex()
		{
			for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
			{
				if (System.Windows.Forms.Screen.AllScreens[i] == Area.Screen)
					return i;
			}

			return 0;
		}

		private void Start()
		{
			controller = null;
			presentation.start();

			int i = 1;
			while (controller == null && i < 150)
			{
				System.Threading.Thread.Sleep(100);
				i++;
				controller = presentation.getController();
			}

			if (controller == null)
				throw new InvalidOperationException(); // TODO (Slideshow.Impress)

			controller.addSlideShowListener(listener);
		}

		private string CreateFileUrl(string path)
		{
			return "file:///" + path.Replace("\\", "/").Replace(":", "|").Replace(" ", "%20");
		}

		private void CreateThumbnails()
		{
			var pages = (document as XDrawPagesSupplier).getDrawPages();
			var file = Path.GetTempFileName();
			for (int i = 0; i < pages.getCount(); i++)
			{
				var page = (XDrawPage)pages.getByIndex(i).Value;
				(document.getCurrentController() as XDrawView).setCurrentPage(page);
				(document as XStorable).storeToURL(CreateFileUrl(file), new PropertyValue[] { CreateProperty("FilterName", new uno.Any("impress_png_Export")) });
				using (StreamReader reader = new StreamReader(file))
				{
					var decoder = new PngBitmapDecoder(reader.BaseStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
					thumbnails.Add(decoder.Frames[0]);
				}
			}
			File.Delete(file);
			(document.getCurrentController() as XDrawView).setCurrentPage((XDrawPage)pages.getByIndex(0).Value);
		}

		private PropertyValue CreateProperty(string name, uno.Any value)
		{
			PropertyValue prop = new PropertyValue();
			prop.Name = name;
			prop.Value = value;
			return prop;
		}

		public override void GotoSlide(int index)
		{
			controller.gotoSlideIndex(index);
		}

		public override void NextStep()
		{
			controller.gotoNextEffect();
		}

		public override void PreviousStep()
		{
			controller.gotoPreviousEffect();
		}

		public override void Show()
		{
			ShowWindow(hWnd, 1);
		}

		public override void Close()
		{
			presentation.end();
			controller = null;
		}

		public override void Hide()
		{
			ShowWindow(hWnd, 0);
		}

		public override IList<ImageSource> Thumbnails
		{
			get
			{
				return thumbnails;
			}
		}

		public override int SlideIndex
		{
			get
			{
				return controller.getCurrentSlideIndex();
			}
		}

		public class SlideShowListener : XSlideShowListener
		{
			public event EventHandler SlideTransitionStarted;
			public event EventHandler SlideTransitionEnded;
			public event EventHandler SlideEnded;

			public void slideTransitionStarted()
			{
				if (SlideTransitionStarted != null)
					SlideTransitionStarted(this, EventArgs.Empty);
			}

			public void slideTransitionEnded()
			{
				if (SlideTransitionEnded != null)
					SlideTransitionEnded(this, EventArgs.Empty);
			}

			public void slideEnded(bool reverse)
			{
				if (SlideEnded != null)
					SlideEnded(this, EventArgs.Empty);
			}

			public void beginEvent(unoidl.com.sun.star.animations.XAnimationNode Node) { }
			public void endEvent(unoidl.com.sun.star.animations.XAnimationNode Node) { }
			public void repeat(unoidl.com.sun.star.animations.XAnimationNode Node, int Repeat) { }
			public void disposing(EventObject Source) { }
			public void hyperLinkClicked(string hyperLink) { }
			public void paused() { }
			public void resumed() { }
			public void slideAnimationsEnded() { }
		}
	}
}
#endif