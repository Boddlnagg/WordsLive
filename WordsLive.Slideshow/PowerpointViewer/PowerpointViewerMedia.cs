using WordsLive.Core;
using System.Collections.Generic;
using System.IO;

namespace WordsLive.Slideshow.PowerpointViewer
{
	public class PowerpointViewerMedia : SlideshowMedia
	{
		public PowerpointViewerMedia(string file) : base(file) { }

		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PowerpointViewerPresentation>();
			pres.Init(this);
			return pres;
		}

		public override void Load()
		{
			// do nothing (loading is done in presentation)
		}
	}
}
