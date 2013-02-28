using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public SlideshowMedia(string file, IMediaDataProvider provider) : base(null) { } // TODO!!

		public abstract ISlideshowPresentation CreatePresentation();
	}
}
