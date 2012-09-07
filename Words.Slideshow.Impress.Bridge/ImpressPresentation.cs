using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using unoidl.com.sun.star.awt;
using unoidl.com.sun.star.beans;
using unoidl.com.sun.star.drawing;
using unoidl.com.sun.star.frame;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.presentation;

namespace Words.Slideshow.Impress.Bridge
{			
	/// <summary>
	/// Bridge to Open-/LibreOffice Impress
	/// </summary>
	public class ImpressPresentation : SlideshowPresentationBase
	{
		#region P/Invokes
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		/*[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
		
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
		XComponent component;
		SlideShowListener listener = new SlideShowListener();
		IntPtr hWnd;
		bool presentationEnded;

		ImpressMedia media;

		List<SlideThumbnail> thumbnails = new List<SlideThumbnail>();

		public void Init(ImpressMedia media)
		{
			this.media = media;
		}

		public override bool Load()
		{
			try
			{
				// Start LibreOffice and load file
				unoidl.com.sun.star.uno.XComponentContext localContext = uno.util.Bootstrap.bootstrap();
				unoidl.com.sun.star.lang.XMultiServiceFactory multiServiceFactory = (unoidl.com.sun.star.lang.XMultiServiceFactory)localContext.getServiceManager();
				XComponentLoader componentLoader = (XComponentLoader)multiServiceFactory.createInstance("com.sun.star.frame.Desktop");
				component = componentLoader.loadComponentFromURL(CreateFileUrl(media.File), "_blank", 0, new PropertyValue[] { });


				// Hide the window and set display
				document = (XModel)component;
				XWindow window = document.getCurrentController().getFrame().getContainerWindow();
				window.setVisible(false);
				XSystemDependentWindowPeer xWindowPeer = (XSystemDependentWindowPeer)(window);
				var handle = xWindowPeer.getWindowHandle(new byte[] { }, 1);
				presentation = (XPresentation2)((XPresentationSupplier)component).getPresentation();

				CreateThumbnails();

				listener.SlideTransitionStarted += (sender, args) =>
				{
					OnSlideIndexChanged();
				};

				listener.SlideEnded += (sender, args) =>
				{
					if (controller.getNextSlideIndex() == -1) // presentation has ended
					{
						presentationEnded = true;
					}
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

				Controller.FocusMainWindow();

				return true;
			}
			catch
			{
				return false;
			}
		}

		private int GetDisplayIndex()
		{
			for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
			{
				if (System.Windows.Forms.Screen.AllScreens[i] == Area.Screen)
					return i + 1;
			}

			return 0;
		}

		private void Start()
		{
			// TODO (Slideshow.Impress): when restarting (after presentation has finished),
			// the Presenter Console is shown -> find that window/frame and hide it
			controller = null;
			presentation.setPropertyValue("Display", new uno.Any(GetDisplayIndex()));
			var props = ((XPropertySetInfo)presentation.getPropertySetInfo()).getProperties();
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
					decoder.Frames[0].Freeze();
					thumbnails.Add(new SlideThumbnail { Image = decoder.Frames[0], Title = "Folie "+(i+1)});
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
			if (presentationEnded || controller.getCurrentSlideIndex() == -1)
			{
				controller.removeSlideShowListener(listener);
				presentation.end();
				Start();
				Controller.FocusMainWindow();
				presentationEnded = false;
			}
			controller.gotoSlideIndex(index);
		}

		public override void NextStep()
		{
			if (!presentationEnded)
				controller.gotoNextEffect();
		}

		public override void PreviousStep()
		{
			if (!presentationEnded)
				controller.gotoPreviousEffect();
		}

		public override void Show()
		{
			ShowWindow(hWnd, 1);
			Controller.FocusMainWindow();
		}

		public override void Close()
		{
			try
			{
				presentation.end();
				component.dispose();
			}
			catch (DisposedException)
			{
				// ignore
			}
			controller = null;
		}

		public override void Hide()
		{
			ShowWindow(hWnd, 0);
		}

		public override IList<SlideThumbnail> Thumbnails
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
	
		
	}
}