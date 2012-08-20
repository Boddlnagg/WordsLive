using System;
using System.Collections.Generic;
using Words.Presentation;

namespace Words.Slideshow
{
	public interface ISlideshowPresentation : IPresentation
	{
		bool Load();
		void GotoSlide(int index);
		void NextStep();
		void PreviousStep();
		int SlideIndex { get; }
		IList<SlideThumbnail> Thumbnails { get; }
		event EventHandler Loaded;
		event EventHandler SlideIndexChanged;
		event EventHandler ClosedExternally;
	}
}
