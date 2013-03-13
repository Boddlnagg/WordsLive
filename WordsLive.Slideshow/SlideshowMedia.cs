using System;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public SlideshowMedia(Uri uri) : base(uri) { }

		public abstract ISlideshowPresentation CreatePresentation();
	}
}
