﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using unoidl.com.sun.star.awt;
using unoidl.com.sun.star.beans;
using unoidl.com.sun.star.container;
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

		/*[DllImport("user32.dll", SetLastError = true)]
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
		IntPtr presentationHandle, mainHandle;
		bool presentationEnded;
		bool isShown;

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

				// Get the main window's handle and hide the window
				document = (XModel)component;
				XWindow window = document.getCurrentController().getFrame().getContainerWindow();
				window.setVisible(false);
				XSystemDependentWindowPeer xWindowPeer = (XSystemDependentWindowPeer)(window);
				mainHandle = new IntPtr((int)xWindowPeer.getWindowHandle(new byte[] { }, SystemDependent.SYSTEM_WIN32).Value);
				ShowWindow(mainHandle, 0);

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
			//var bit = GetClassLongPtr(hWnd, -26).ToInt32() | CS_DROPSHADOW;
			//SetClassLong(hWnd, -26, new IntPtr(GetClassLongPtr(hWnd, -26).ToInt32() ^ CS_DROPSHADOW));
			//bit = GetClassLongPtr(hWnd, -26).ToInt32() | CS_DROPSHADOW;

			// Resizing the window works (but a dropshadow remains) and moving doesn't, so we stay fullscreen
			//MoveWindow(hWnd, 0, 0, 800, 600, true);

			ShowWindow(presenterConsoleHandle, 0); // hide presenter console

			if(!isShown)
				ShowWindow(presentationHandle, 0); // hide presentation window if needed
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
			return "file:///" + path.Replace("\\", "/").Replace(":", "|").Replace(" ", "%20");
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
					thumbnails.Add(new SlideThumbnail { Image = decoder.Frames[0], Title = name + " (Folie "+(i+1)+")"});
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
			isShown = true;
			ShowWindow(presentationHandle, 1);
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
	
		
	}
}