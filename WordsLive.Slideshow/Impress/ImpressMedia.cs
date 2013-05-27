using System;
using System.IO;

namespace WordsLive.Slideshow.Impress
{
	public class ImpressMedia : SlideshowMedia
	{
		Type presentationType;

		public ImpressMedia(Uri uri, Type presentationType) : base(uri)
		{
			this.presentationType = presentationType;
		}
		
		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = (ISlideshowPresentation)Controller.PresentationManager.CreatePresentation(presentationType);
			presentationType.GetMethod("Init", new Type[] { typeof(FileInfo) }).Invoke(pres, new object[] { new FileInfo(Uri.LocalPath) });
			return pres;
		}

		public override void Load()
		{
			if (!Uri.IsFile)
			{
				// TODO: temporarily download file
				throw new NotImplementedException("Loading presentations from remote URIs is not yet implemented.");
			}

			if (!File.Exists(Uri.LocalPath))
			{
				throw new FileNotFoundException();
			}
		}
	}
}
