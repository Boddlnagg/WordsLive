using Words.Core;

namespace Words.Slideshow.Powerpoint
{
	[FileExtension(".ppt")]
	[FileExtension(".pptx")] // TODO (Slideshow.PowerpointViewer): untested
	public class PowerpointViewerMedia : SlideshowMedia
	{
		protected override bool LoadFromMetadata()
		{
			return PowerpointViewerLib.PowerpointViewerController.IsAvailable;
		}

		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PowerpointViewerPresentation>();
			pres.Init(this);
			return pres;
		}
	}
}
