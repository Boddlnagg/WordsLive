using System;

namespace Words.Slideshow.Impress
{
	public class ImpressMedia : SlideshowMedia
	{
		Type presentationType;

		public ImpressMedia(Type presentationType)
		{
			this.presentationType = presentationType;
		}
		
		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = (ISlideshowPresentation)Controller.PresentationManager.CreatePresentation(presentationType);
			presentationType.GetMethod("Init", new Type[] { typeof(ImpressMedia) }).Invoke(pres, new object[] { this });
			return pres;
		}
	}
}
