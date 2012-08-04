using Words.Core;

namespace Words.Slideshow.Impress
{
	[MediaType("OpenDocument Presentation", ".odp")]
	public class ImpressMedia : SlideshowMedia
	{
		protected override bool LoadFromMetadata()
		{
			return true; // TODO (Slideshow.Impress): check if impress is available
		}

		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<ImpressPresentation>();
			pres.Init(this);
			return pres;
		}
	}
}
