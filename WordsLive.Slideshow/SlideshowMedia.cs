using WordsLive.Core;

namespace WordsLive.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public SlideshowMedia(string file) : base(file) { }

		public abstract ISlideshowPresentation CreatePresentation();
	}
}
