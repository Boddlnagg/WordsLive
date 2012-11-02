using WordsLive.Core;
using System.Collections.Generic;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow.PowerpointViewer
{
	public class PowerpointViewerMedia : SlideshowMedia
	{
		public FileInfo LocalFile { get; private set; }

		public PowerpointViewerMedia(string file, MediaDataProvider provider) : base(file, provider) { }

		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PowerpointViewerPresentation>();
			pres.Init(LocalFile);
			return pres;
		}

		public override void Load()
		{
			// get the local file (this will temporarily download the file if needed)
			LocalFile = this.DataProvider.GetLocal(this.File);
		}
	}
}
