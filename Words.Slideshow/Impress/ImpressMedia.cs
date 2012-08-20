using Words.Core;
using System.Collections.Generic;
using System.IO;

namespace Words.Slideshow.Impress
{
	public class ImpressMedia : SlideshowMedia
	{
		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<ImpressPresentation>();
			pres.Init(this);
			return pres;
		}
	}
}
