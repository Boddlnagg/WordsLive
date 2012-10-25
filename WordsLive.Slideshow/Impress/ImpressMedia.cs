using System;

namespace WordsLive.Slideshow.Impress
{
	public class ImpressMedia : SlideshowMedia
	{
		Type presentationType;

		public ImpressMedia(string file, Type presentationType) : base(file)
		{
			this.presentationType = presentationType;
		}
		
		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = (ISlideshowPresentation)Controller.PresentationManager.CreatePresentation(presentationType);
			presentationType.GetMethod("Init", new Type[] { typeof(ImpressMedia) }).Invoke(pres, new object[] { this });
			return pres;
		}

		public override void Load()
		{
			// do nothing (loading is done in presentation)
		}
	}
}
