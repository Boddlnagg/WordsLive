using System;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow.Impress
{
	public class ImpressMedia : SlideshowMedia
	{
		Type presentationType;

		public FileInfo LocalFile { get; private set; }

		public ImpressMedia(string file, MediaDataProvider provider, Type presentationType) : base(file, provider)
		{
			this.presentationType = presentationType;
		}
		
		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = (ISlideshowPresentation)Controller.PresentationManager.CreatePresentation(presentationType);
			presentationType.GetMethod("Init", new Type[] { typeof(FileInfo) }).Invoke(pres, new object[] { this.LocalFile });
			return pres;
		}

		public override void Load()
		{
			// get the local file (this will temporarily download the file if needed)
			LocalFile = this.DataProvider.GetLocal(this.File);
		}
	}
}
