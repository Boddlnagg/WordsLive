using Words.Core;

namespace Words.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public abstract ISlideshowPresentation CreatePresentation();
	}
}
