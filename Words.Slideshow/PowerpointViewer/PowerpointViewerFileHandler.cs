using System.Collections.Generic;
using System.IO;
using Words.Core;

namespace Words.Slideshow.PowerpointViewer
{
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

		public override Media TryHandle(FileInfo file)
		{
			if (!PowerpointViewerLib.PowerpointViewerController.IsAvailable)
				return null;

			var media = new PowerpointViewerMedia();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
