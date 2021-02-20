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
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Office.Core;
using WordsLive.Presentation.Wpf;
using WordsLive.Slideshow.Presentation;
using WordsLive.Resources;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace WordsLive.Slideshow.Powerpoint.Bridge
{
	public class PowerpointPresentation : SlideshowPresentationBase
	{
		private const float OUTSIDE_Y = -20000.25f;
		private FileInfo file;
		private PowerPoint.Application application;
		private PowerPoint.Presentation presentation;
		private List<SlideThumbnail> thumbnails;

		/// <summary>
		/// This is called via reflection. It checks, whether we can create a PowerPoint Application COM-binding,
		/// because we might be able to load the assembly even if PowerPoint is not installed and therefore
		/// this check is needed.
		/// </summary>
		/// <returns></returns>
		public static bool CheckAvailability()
		{
			try
			{
				new PowerPoint.Application();
				return true;
			}
			catch (COMException)
			{
				return false;
			}
		}

		public void Init(FileInfo file)
		{
			this.file = file;
		}

		public override void Load()
		{
			Area.WindowSizeChanged += Area_WindowSizeChanged;
			Area.WindowLocationChanged += Area_WindowLocationChanged;

			var task = Task.Factory.StartNew(PerformLoad);
			task.ContinueWith(t =>
				Controller.Dispatcher.Invoke(new Action(() => { base.OnLoaded(false); throw new Exception("Exception occured while loading presentation", t.Exception.InnerException); })),
				TaskContinuationOptions.OnlyOnFaulted);
		}

		private void PerformLoad()
		{
			try
			{
				this.application = new PowerPoint.Application();
				this.presentation = application.Presentations.Open(file.FullName, MsoTriState.msoTrue, MsoTriState.msoTrue, MsoTriState.msoFalse);

				this.application.SlideShowNextSlide += application_SlideShowNextSlide;
				this.application.SlideShowEnd += application_SlideShowEnd;

				this.presentation.SlideShowSettings.Run();
				this.presentation.SlideShowWindow.Top = OUTSIDE_Y; // outside of screen (-> hidden)
				this.presentation.SlideShowWindow.Left = PixelsToPoints(Area.WindowLocation.X);
				this.presentation.SlideShowWindow.Height = PixelsToPoints(Area.WindowSize.Height);
				this.presentation.SlideShowWindow.Width = PixelsToPoints(Area.WindowSize.Width);

				CreateThumbnails();

				LoadPreviewProvider();

				base.OnLoaded(true);
			}
			catch (COMException)
			{
				base.OnLoaded(false);
			}
		}

		void application_SlideShowEnd(PowerPoint.Presentation Pres)
		{
			try
			{
				if (Pres.SlideShowWindow.HWND == this.presentation.SlideShowWindow.HWND)
				{
					Cleanup();
					base.OnClosedExternally();
				}
			}
			catch (COMException) { }
		}

		void application_SlideShowNextSlide(PowerPoint.SlideShowWindow Wn)
		{
			if (Wn == this.presentation.SlideShowWindow)
			{
				base.OnSlideIndexChanged();
			}
		}

		private float PixelsToPoints(int px)
		{
			return px / 96f * 72f; // TODO: don't hardcode DPI of 96
		}

		void CreateThumbnails()
		{
			thumbnails = new List<SlideThumbnail>();
			var file = Path.GetTempFileName();

			for (int i = 0; i < this.presentation.Slides.Count; i++)
			{
				Microsoft.Office.Interop.PowerPoint.Slide slide = this.presentation.Slides[i + 1];
				slide.Export(file, "PNG", 800);
				using (StreamReader reader = new StreamReader(file))
				{
					var decoder = new PngBitmapDecoder(reader.BaseStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
					decoder.Frames[0].Freeze();
					thumbnails.Add(new SlideThumbnail { Image = decoder.Frames[0], Title = slide.Name + " (" + String.Format(Resource.slideDescriptionN, i + 1) + ")" });
				}
			}

			File.Delete(file);
		}

		private void Area_WindowSizeChanged(object sender, EventArgs e)
		{
			this.presentation.SlideShowWindow.Height = PixelsToPoints(Area.WindowSize.Height);
			this.presentation.SlideShowWindow.Width = PixelsToPoints(Area.WindowSize.Width);
		}

		private void Area_WindowLocationChanged(object sender, EventArgs e)
		{
			if (this.presentation.SlideShowWindow.Top != OUTSIDE_Y)
			{
				this.presentation.SlideShowWindow.Top = PixelsToPoints(Area.WindowLocation.Y);
			}

			this.presentation.SlideShowWindow.Left = PixelsToPoints(Area.WindowLocation.X);
		}

		protected override void LoadPreviewProvider()
		{
			Controller.Dispatcher.Invoke((Action)delegate
			{
				preview = new LiveWindowPreviewProvider(new IntPtr(this.presentation.SlideShowWindow.HWND));
			});
		}

		public override void GotoSlide(int index)
		{
			try
			{
				this.presentation.SlideShowWindow.View.GotoSlide(index + 1);
			}
			catch (COMException) { }
		}

		public override void NextStep()
		{
			try
			{
				this.presentation.SlideShowWindow.View.Next();
			}
			catch (COMException) { }
		}

		public override void PreviousStep()
		{
			try
			{
				this.presentation.SlideShowWindow.View.Previous();
			}
			catch (COMException) { }
		}

		public override void Show()
		{
			this.presentation.SlideShowWindow.Top = PixelsToPoints(Area.WindowLocation.Y);
		}

		public override void Hide()
		{
			this.presentation.SlideShowWindow.Top = OUTSIDE_Y;
		}

		public override IList<SlideThumbnail> Thumbnails
		{
			get
			{
				return thumbnails;
			}
		}

		public override bool IsEndless
		{
			get
			{
				return this.presentation.SlideShowSettings.LoopUntilStopped == MsoTriState.msoTrue;
			}
		}

		public override int SlideIndex
		{
			get
			{
				try
				{
					return this.presentation.SlideShowWindow.View.Slide.SlideIndex - 1;
				} catch (COMException)
				{
					return -1;
				}
			}
		}

		public override void Close()
		{
			Cleanup();
			try
			{
				if (this.presentation != null)
					this.presentation.Close();
			}
			catch (COMException) { }
		}

		private void Cleanup()
		{
			Area.WindowSizeChanged -= Area_WindowSizeChanged;
			Area.WindowLocationChanged -= Area_WindowLocationChanged;
			this.application.SlideShowNextSlide -= application_SlideShowNextSlide;
			this.application.SlideShowEnd -= application_SlideShowEnd;
		}
	}
}
