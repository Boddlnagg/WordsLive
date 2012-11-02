using System;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow.Impress
{
	public class ImpressMedia : SlideshowMedia
	{
		Type presentationType;

		public ImpressMedia(string file, MediaDataProvider provider, Type presentationType) : base(file, provider)
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
