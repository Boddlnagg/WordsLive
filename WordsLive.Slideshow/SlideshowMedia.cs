using WordsLive.Core;

namespace WordsLive.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public abstract ISlideshowPresentation CreatePresentation();
	}
}
