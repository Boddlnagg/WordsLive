using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;

namespace Words.Slideshow
{
	public abstract class SlideshowMedia : Media
	{
		public abstract ISlideshowPresentation CreatePresentation();
	}
}
