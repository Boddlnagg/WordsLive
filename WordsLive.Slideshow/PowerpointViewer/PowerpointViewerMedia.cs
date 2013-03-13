using System;
using System.IO;

namespace WordsLive.Slideshow.PowerpointViewer
{
	public class PowerpointViewerMedia : SlideshowMedia
	{
		public FileInfo LocalFile { get; private set; }

		public PowerpointViewerMedia(Uri uri) : base(uri) { }

		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PowerpointViewerPresentation>();
			pres.File = new FileInfo(Uri.LocalPath);
			return pres;
		}

		public override void Load()
		{
			if (!Uri.IsFile)
			{
				// TODO: temporarily download file
				throw new NotImplementedException("Loading presentations from remote URIs is not yet implemented.");
			}
		}
	}
}
