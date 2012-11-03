﻿using System.Collections.Generic;
using System.IO;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow.PowerpointViewer
{
	// TODO: This currently only works with PowerpointViewer 2007 (older versions untested), not with 2010
	// and Visual C++ 2010 Redistributable must be installed for pptviewlib.dll to work
	public class PowerpointFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".ppt", ".pptx" }; } // TODO (Slideshow.PowerpointViewer): pptx untested
		}

		public override string Description
		{
			get { return "Powerpoint-Präsentationen"; }
		}

		public override Media TryHandle(string path, IMediaDataProvider provider)
		{
			if (!PowerpointViewerLib.PowerpointViewerController.IsAvailable)
				return null;

			return new PowerpointViewerMedia(path, provider);
		}
	}
}
