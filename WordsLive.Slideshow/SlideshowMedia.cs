using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public SlideshowMedia(string file, MediaDataProvider provider) : base(file, provider) { }

		public abstract ISlideshowPresentation CreatePresentation();
	}
}
