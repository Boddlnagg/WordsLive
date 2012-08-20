using Words.Core;
using System.Collections.Generic;
using System.IO;

namespace Words.Slideshow.PowerpointViewer
{
	public class PowerpointViewerMedia : SlideshowMedia
	{
		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PowerpointViewerPresentation>();
			pres.Init(this);
			return pres;
		}
	}
}
