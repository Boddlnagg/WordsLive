/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using unoidl.com.sun.star.awt;
using unoidl.com.sun.star.beans;
using unoidl.com.sun.star.container;
using unoidl.com.sun.star.drawing;
using unoidl.com.sun.star.frame;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.presentation;
using WordsLive.Presentation.Wpf;
using WordsLive.Slideshow.Presentation;
using WordsLive.Slideshow.Resources;

namespace WordsLive.Slideshow.Impress.Bridge
{			
	/// <summary>
	/// Bridge to Open-/LibreOffice Impress
	/// </summary>
	public class ImpressPresentation : SlideshowPresentationBase
	{
		// TODO: try to maintain correct window z-order using SetWindowPos, especially for ImpressPresentation
		// (see http://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx)

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
		private static extern int GetWindowRect(IntPtr hwnd, out Rect rc);

		public struct Rect
		{
			public Rect(int x, int y, int x1, int y1)
			{
				this.Left = x;
				this.Top = y;
				this.Right = x1;
				this.Bottom = y1;
			}

			public int Left, Top, Right, Bottom;
		}
		#endregion

		XPresentation2 presentation;
		XSlideShowController controller;
		XModel document;
		XComponent component;
		XDesktop desktop;
		SlideShowListener listener = new SlideShowListener();
		IntPtr presentationHandle, mainHandle;
		bool isShown;

		FileInfo file;

		List<SlideThumbnail> thumbnails = new List<SlideThumbnail>();

		private bool? isEndless = null;

		public override bool IsEndless
		{
			get
			{
				if (!isEndless.HasValue)
					throw new InvalidOperationException("Slideshow not loaded yet.");

				return isEndless.Value;
			}
		}

		public void Init(FileInfo file)
		{
			this.file = file;
		}

		public override void Load()
		{
			// TODO: improve error handling (return Task instead?)

			Area.WindowSizeChanged += Area_WindowSizeChanged;

			var task = Task.Factory.StartNew(PerformLoad);
			task.ContinueWith(t =>
				Controller.Dispatcher.Invoke(new Action(() => { base.OnLoaded(false); throw new Exception("Exception occured while loading presentation", t.Exception.InnerException); })),
				TaskContinuationOptions.OnlyOnFaulted);
		}

		void PerformLoad()
		{
			// Start LibreOffice and load file
			unoidl.com.sun.star.uno.XComponentContext localContext = uno.util.Bootstrap.bootstrap();
			unoidl.com.sun.star.lang.XMultiServiceFactory multiServiceFactory = (unoidl.com.sun.star.lang.XMultiServiceFactory)localContext.getServiceManager();
			desktop = (XDesktop)multiServiceFactory.createInstance("com.sun.star.frame.Desktop");
			var componentLoader = (XComponentLoader)desktop;
			component = componentLoader.loadComponentFromURL(CreateFileUrl(file.FullName), "_blank", 0, new PropertyValue[] { });

			// TODO: while/before loading, set the viewsettings ViewId:=view1, PageKind:=0 to reset to the default view
			// (instead of notes or outline view). Unfortunately these settings aren't accesible in the API ...

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

			Start();
			controller.gotoSlideIndex(0);

			LoadPreviewProvider();

			base.OnLoaded(true);
		}

		int? restoreSlideIndex = null;

		void Area_WindowSizeChanged(object sender, EventArgs e)
		{
			if (presentationHandle != IntPtr.Zero)
			{
				ResizeWindow();
			}
		}

		private void SetDisplay()
		{
			presentation.setPropertyValue("Display", new uno.Any(GetAreaDisplayIndex()));
		}

		private void ResizeWindow()
		{
			// MoveWindow will set the correct size, but only if the location is the displays's top-left corner.
			// Furthemore when we resize the window, a dropshadow remains, because the window class has CS_DROPSHADOW enabled and we can't change the window class.
			// Therefore we use Area.Screen.Bounds instead of Area.WindowLocation
			MoveWindow(presentationHandle, Area.Screen.Bounds.Left, Area.Screen.Bounds.Top, Area.WindowSize.Width, Area.WindowSize.Height, true);

			// wait a moment and resize again (this is needed, for when the display changes,
			// the first call will only move it to the correct screen but leave the window in fullscreen mode)
			Thread.Sleep(100);
			MoveWindow(presentationHandle, Area.Screen.Bounds.Left, Area.Screen.Bounds.Top, Area.WindowSize.Width, Area.WindowSize.Height, true);

			//IntPtr child = FindWindowEx(presentationHandle, IntPtr.Zero, "SALOBJECT", null);
			//IntPtr child2 = FindWindowEx(child, IntPtr.Zero, "SALOBJECTCHILD", null);
		}

		private int GetAreaDisplayIndex()
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

			SetDisplay();

			presentation.start();

			int i = 1;
			while (controller == null && i < 150)
			{
				Thread.Sleep(100);
				i++;
				controller = presentation.getController();
			}

			if (controller == null)
				throw new InvalidOperationException();

			controller.addSlideShowListener(listener);

			if (!isEndless.HasValue)
				isEndless = controller.isEndless();

			controller.gotoSlideIndex(restoreSlideIndex.HasValue ? restoreSlideIndex.Value : 0);

			IntPtr presenterConsoleHandle;

			GetWindowHandles(out presenterConsoleHandle, out presentationHandle);

			WordsLive.Presentation.Wpf.Interop.RemoveFromAeroPeek(presentationHandle);

			if (presenterConsoleHandle != IntPtr.Zero)
			{
				ShowWindow(presenterConsoleHandle, 0); // hide presenter console
			}

			ResizeWindow();

			if (!isShown)
			{
				ShowWindow(presentationHandle,11); // hide presentation window if needed // 0

				IntPtr child = FindWindowEx(presentationHandle, IntPtr.Zero, "SALOBJECT", null);
				IntPtr child2 = FindWindowEx(child, IntPtr.Zero, "SALOBJECTCHILD", null);

				ShowWindow(child, 11);
			}

			if (preview != null)
			{
				(preview as LiveWindowPreviewProvider).UpdateSource(presentationHandle);
			}

			Controller.Dispatcher.Invoke(new Action(() => Controller.FocusMainWindow()));
		}

		private void GetWindowHandles(out IntPtr presenterConsoleHandle, out IntPtr presentationHandle)
		{
			presenterConsoleHandle = FindWindow("SALTMPSUBFRAME", GetWindowTitle(mainHandle));
			IntPtr parent = presenterConsoleHandle;
			IntPtr child = IntPtr.Zero;

			int i = 0;

			while (child == IntPtr.Zero && i++ < 100)
			{
				parent = FindWindowEx(IntPtr.Zero, parent, "SALTMPSUBFRAME", null);
				child = FindWindowEx(parent, IntPtr.Zero, "SALOBJECT", null);
			}
			if (child == IntPtr.Zero)
			{
				throw new InvalidOperationException("Couldn't find presentation window.");
			}

			presentationHandle = parent;
		}

		private string CreateFileUrl(string path)
		{
			return new Uri(path).AbsoluteUri;
		}

		private void CreateThumbnails()
		{
			// TODO: this currently only works when the default view is selected (not outline or notes)
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
				if (!RestartIfNecessary())
				{
					restoreSlideIndex = controller.getCurrentSlideIndex();
				}

				// use a new task here to work around an issue with multiple successive calls to this method
				// (callee freezes, might be a deadlock)
				Task.Factory.StartNew(() => controller.gotoSlideIndex(index));
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
				if (!RestartIfNecessary())
				{
					restoreSlideIndex = controller.getCurrentSlideIndex();

					bool isPaused = controller.isPaused();

					// use a new task here to work around an issue with multiple successive calls to this method
					// (callee freezes, might be a deadlock)
					Task.Factory.StartNew(() => 
					{
						controller.gotoNextEffect();
						if (!isPaused)
						{
							Thread.Sleep(50);
							if (controller.isPaused())
							{
								// gotoPreviousEffect needs to be called multiple times if
								// there are effects on the last slide
								controller.gotoPreviousSlide();
							}
						}
					});
				}
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
				if (!RestartIfNecessary())
				{
					restoreSlideIndex = controller.getCurrentSlideIndex();
					// use a new task here to work around an issue with multiple successive calls to this method
					// (callee freezes, might be a deadlock)
					Task.Factory.StartNew(() => controller.gotoPreviousEffect());
				}
			}
			catch (DisposedException)
			{
				OnClosedExternally();
			}
		}

		private bool RestartIfNecessary()
		{
			if (controller.getCurrentSlideIndex() == -1)
			{
				controller.removeSlideShowListener(listener);
				presentation.end();
				Start();
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override void LoadPreviewProvider()
		{
			Controller.Dispatcher.Invoke((Action)delegate
			{
				preview = new LiveWindowPreviewProvider(presentationHandle);
			});
		}

		public override void Show()
		{
			isShown = true;
			ShowWindow(presentationHandle, 4); // 8
			if (!Area.Fullscreen)
			{
				ResizeWindow();
			}
			Controller.FocusMainWindow();
		}

		public override void Close()
		{
			Area.WindowSizeChanged -= Area_WindowSizeChanged;
			Task.Factory.StartNew(() => {
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
			}).ContinueWith((t) => 
			{
				controller = null;

				base.Close();
			});
		}

		public override void Hide()
		{
			isShown = false;
			ShowWindow(presentationHandle, 11); // 0
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
				try
				{
					return controller.getCurrentSlideIndex();
				}
				catch (DisposedException)
				{
					return -1;
				}
			}
		}
	}
}