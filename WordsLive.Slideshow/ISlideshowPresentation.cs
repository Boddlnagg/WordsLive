using System;
using System.Collections.Generic;
using WordsLive.Presentation;
using System.Windows.Media.Imaging;

namespace WordsLive.Slideshow
{
	public class SlideshowLoadedEventArgs : EventArgs
	{
		public bool Success { get; private set; }

		public SlideshowLoadedEventArgs(bool success)
		{
			Success = success;
		}
	}

	public interface ISlideshowPresentation : IPresentation
	{
		void Load();
		void GotoSlide(int index);
		void NextStep();
		void PreviousStep();
		BitmapSource CaptureWindow(int width);
		int SlideIndex { get; }
		IList<SlideThumbnail> Thumbnails { get; }
		event EventHandler<SlideshowLoadedEventArgs> Loaded;
		event EventHandler SlideIndexChanged;
		event EventHandler ClosedExternally;
	}
}
