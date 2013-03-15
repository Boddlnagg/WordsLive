using System;
using WordsLive.Core;

namespace WordsLive.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public SlideshowMedia(Uri uri) : base(uri) { }

		public abstract ISlideshowPresentation CreatePresentation();
	}
}
