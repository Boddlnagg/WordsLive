using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.Slideshow.PowerpointViewer
{
	// TODO: This currently only works with PowerpointViewer 2007 (older versions untested), not with 2010
	// and Visual C++ 2010 Redistributable must be installed for pptviewlib.dll to work
	public class PowerpointHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".ppt", ".pptx" }; } // TODO (Slideshow.PowerpointViewer): pptx untested
		}

		public override string Description
		{
			get { return "Powerpoint-Präsentationen"; }
		}

		public override int Test(Uri uri)
		{
			if (!PowerpointViewerLib.PowerpointViewerController.IsAvailable)
				return -1;
			else
				return CheckExtension(uri) ? 100 : -1;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new PowerpointViewerMedia(uri);
		}
	}
}
