using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using unoidl.com.sun.star.awt;
using unoidl.com.sun.star.beans;
using unoidl.com.sun.star.container;
using unoidl.com.sun.star.drawing;
using unoidl.com.sun.star.frame;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.presentation;
using WordsLive.Slideshow.Resources;

namespace WordsLive.Slideshow.Impress.Bridge
{			
	/// <summary>
	/// Bridge to Open-/LibreOffice Impress
	/// </summary>
	public class ImpressPresentation : SlideshowPresentationBase
	{
		#region P/Invokes
		[DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		public static extern int GetWindowTextLength(HandleRef hWnd);

		[DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		/*[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		public static IntPtr SetClassLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size > 4)
				return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
			else
				return new IntPtr(SetClassLongPtr32(hWnd, nIndex, unchecked((uint)dwNewLong.ToInt32())));
		}

		[DllImport("user32.dll", EntryPoint="SetClassLong", SetLastError=true)]
		public static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

		[DllImport("user32.dll", EntryPoint="SetClassLongPtr", SetLastError=true)]
		public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size > 4)
				return GetClassLongPtr64(hWnd, nIndex);
			else
				return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
		}

		[DllImport("user32.dll", EntryPoint="GetClassLong", SetLastError=true)]
		public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint="GetClassLongPtr", SetLastError=true)]
		public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);
		*/

		[DllImport("user32.dll")]
		private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

		[DllImport("user32.dll")]
		private static extern int GetWindowRect(IntPtr hwnd, out System.Drawing.Rectangle rc);
		#endregion

		XPresentation2 presentation;
		XSlideShowController controller;
		XModel document;
		XComponent component;
		XDesktop desktop;
		SlideShowListener listener = new SlideShowListener();
		IntPtr presentationHandle, mainHandle;
		bool presentationEnded;
		bool isShown;

		FileInfo file;

		List<SlideThumbnail> thumbnails = new List<SlideThumbnail>();

		public void Init(FileInfo file)
		{
			this.file = file;
		}

		public override void Load()
		{
			try
			{
				Area.WindowSizeChanged += Area_WindowSizeChanged;

				ThreadPool.QueueUserWorkItem(new WaitCallback(PerformLoad));

				Controller.FocusMainWindow();
			}
			catch
			{
				base.OnLoaded(false);
			}
		}

		void PerformLoad(object o)
		{
			// Start LibreOffice and load file
			unoidl.com.sun.star.uno.XComponentContext localContext = uno.util.Bootstrap.bootstrap();
			unoidl.com.sun.star.lang.XMultiServiceFactory multiServiceFactory = (unoidl.com.sun.star.lang.XMultiServiceFactory)localContext.getServiceManager();
			desktop = (XDesktop)multiServiceFactory.createInstance("com.sun.star.frame.Desktop");
			var componentLoader = (XComponentLoader)desktop;
			component = componentLoader.loadComponentFromURL(CreateFileUrl(file.FullName), "_blank", 0, new PropertyValue[] { });

			// Get the main window's handle and hide the window
			document = (XModel)component;
			XWindow window = document.getCurrentController().getFrame().getContainerWindow();
			window.setVisible(false);
			XSystemDependentWindowPeer xWindowPeer = (XSystemDependentWindowPeer)(window);
			mainHandle = new IntPtr((int)xWindowPeer.getWindowHandle(new byte[] { }, SystemDependent.SYSTEM_WIN32).Value);
			//ShowWindow(mainHandle, 0);

			presentation = (XPresentation2)((XPresentationSupplier)component).getPresentation();

			CreateThumbnails();

			listener.SlideTransitionStarted += (sender, args) =>
			{
				OnSlideIndexChanged();
			};

			listener.Paused += (sender, args) =>
			{
				presentationEnded = true;
			};

			Start();
			controller.gotoSlideIndex(0);

			LoadPreviewProvider();

			base.OnLoaded(true);
		}

		void Area_WindowSizeChanged(object sender, EventArgs e)
		{
			if (presentationHandle != IntPtr.Zero)
				MoveWindow(presentationHandle, 0, 0, Area.WindowSize.Width, Area.WindowSize.Height, true);
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

		private string GetWindowTitle(IntPtr hWnd)
		{
			var handleRef = new HandleRef(this, hWnd);
			int capacity = GetWindowTextLength(handleRef) * 2;
			StringBuilder stringBuilder = new StringBuilder(capacity);
			GetWindowText(handleRef, stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}

		private void Start()
		{
			controller = null;
			presentation.setPropertyValue("Display", new uno.Any(GetDisplayIndex()));
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

			controller.gotoSlideIndex(0);

			IntPtr presenterConsoleHandle;

			GetWindowHandles(out presenterConsoleHandle, out presentationHandle);

			//int CS_DROPSHADOW = 0x20000;
			//var bit = GetClassLongPtr(presentationHandle, -26).ToInt32() & CS_DROPSHADOW;
			//SetClassLong(presentationHandle, -26, new IntPtr(GetClassLongPtr(presentationHandle, -26).ToInt32() & ~CS_DROPSHADOW));
			//Console.WriteLine(Marshal.GetLastWin32Error()); // 5 = Access denied
			//bit = GetClassLongPtr(presentationHandle, -26).ToInt32() & CS_DROPSHADOW;


			//IntPtr child = FindWindowEx(presentationHandle, IntPtr.Zero, "SALOBJECT", null);
			//IntPtr child2 = FindWindowEx(child, IntPtr.Zero, "SALOBJECTCHILD", null);

			// Resizing the window works, but a dropshadow remains and moving doesn't, so we'll ignore Area.WindowLocation
			MoveWindow(presentationHandle, 0, 0, Area.WindowSize.Width, Area.WindowSize.Height, true);

			WordsLive.Presentation.Wpf.AeroPeekHelper.RemoveFromAeroPeek(presentationHandle);

			ShowWindow(presenterConsoleHandle, 0); // hide presenter console

			if (!isShown)
			{
				ShowWindow(presentationHandle, 0); // hide presentation window if needed
			}
		}

		private void GetWindowHandles(out IntPtr presenterConsoleHandle, out IntPtr presentationHandle)
		{
			presenterConsoleHandle = FindWindow("SALTMPSUBFRAME", GetWindowTitle(mainHandle));
			IntPtr parent = presenterConsoleHandle;
			IntPtr child = IntPtr.Zero;

			while (child == IntPtr.Zero)
			{
				parent = FindWindowEx(IntPtr.Zero, parent, "SALTMPSUBFRAME", null);
				child = FindWindowEx(parent, IntPtr.Zero, "SALOBJECT", null);
			}
			presentationHandle = parent;
		}

		private string CreateFileUrl(string path)
		{
			return new Uri(path).AbsoluteUri;
		}

		private void CreateThumbnails()
		{
			var pages = (document as XDrawPagesSupplier).getDrawPages();
			var file = Path.GetTempFileName();
			for (int i = 0; i < pages.getCount(); i++)
			{
				var page = (XDrawPage)pages.getByIndex(i).Value;
				var name = (page as XNamed).getName();
				(document.getCurrentController() as XDrawView).setCurrentPage(page);
				(document as XStorable).storeToURL(CreateFileUrl(file), new PropertyValue[] { CreateProperty("FilterName", new uno.Any("impress_png_Export")) });
				using (StreamReader reader = new StreamReader(file))
				{
					var decoder = new PngBitmapDecoder(reader.BaseStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
					decoder.Frames[0].Freeze();
					thumbnails.Add(new SlideThumbnail { Image = decoder.Frames[0], Title = name + " ("+ String.Format(Resource.slideN, i+1)+")"});
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
			try
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
			catch (DisposedException)
			{
				OnClosedExternally();
			}
		}

		public override void NextStep()
		{
			try
			{
				if (!presentationEnded)
					controller.gotoNextEffect();
			}
			catch (DisposedException)
			{
				OnClosedExternally();
			}
		}

		public override void PreviousStep()
		{
			try
			{ 
				if (!presentationEnded)
					controller.gotoPreviousEffect();
			}
			catch (DisposedException)
			{
				OnClosedExternally();
			}
		}

		public override void Show()
		{
			isShown = true;
			ShowWindow(presentationHandle, 8);
			Controller.FocusMainWindow();
		}

		public override void Close()
		{
			try
			{
				if (presentation != null)
					presentation.end();
				if (component != null)
					component.dispose();
				if (desktop != null && desktop.getCurrentComponent() == null) // no component is running anymore
					desktop.terminate();
			}
			catch (DisposedException)
			{
				// ignore
			}
			controller = null;

			base.Close();
		}

		public override void Hide()
		{
			isShown = false;
			ShowWindow(presentationHandle, 0);
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

		private Bitmap Capture(int thumbnailWidth)
		{
			System.Drawing.Rectangle rc;
			GetWindowRect(presentationHandle, out rc);

			Bitmap bm = new Bitmap(Area.WindowSize.Width, Area.WindowSize.Height);
			Graphics g = Graphics.FromImage(bm);
			IntPtr hdc = g.GetHdc();

			PrintWindow(presentationHandle, hdc, 0);

			g.ReleaseHdc(hdc);
			g.Flush();
			g.Dispose();

			if (thumbnailWidth > 0)
			{
				double ratio = (double)Area.WindowSize.Width / Area.WindowSize.Height;
				int thumbnailHeight = (int)(thumbnailWidth / ratio);

				Bitmap result = new Bitmap(thumbnailWidth, thumbnailHeight);
				using (Graphics gg = Graphics.FromImage((Image)result))
					gg.DrawImage(bm, 0, 0, thumbnailWidth, thumbnailHeight);
				bm.Dispose();
				return result;
			}
			else
			{
				return bm; // do not resize
			}
		}

		public override System.Windows.Media.Imaging.BitmapSource CaptureWindow(int width)
		{
			return SlideshowPreviewProvider.ConvertBitmap(Capture(width));
		}
	}
}