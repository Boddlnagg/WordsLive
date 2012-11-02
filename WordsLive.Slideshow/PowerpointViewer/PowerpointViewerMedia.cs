using WordsLive.Core;
using System.Collections.Generic;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow.PowerpointViewer
{
	public class PowerpointViewerMedia : SlideshowMedia
	{
		public PowerpointViewerMedia(string file, MediaDataProvider provider) : base(file, provider) { }

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
