using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Ionic.Zip;
using Words.Core;

namespace Words.Slideshow.Photos
{
	[MediaType("Gepackte Diashows", ".zip")]
	public class PhotosMedia : SlideshowMedia
	{
		public IEnumerable<BitmapImage> Images { get; private set; }

		protected override bool LoadFromMetadata()
		{
			Images = LoadFromZip(new ZipFile(this.File));
			return true;
		}

		private IEnumerable<BitmapImage> LoadFromZip(ZipFile zip)
		{
			foreach (var entry in zip.Entries)
			{
				MemoryStream stream = new MemoryStream();
				entry.Extract(stream);
				stream.Flush();
				stream.Seek(0, SeekOrigin.Begin);

				BitmapImage src = new BitmapImage();
				src.BeginInit();
				src.StreamSource = stream;
				src.EndInit();
				yield return src;
			}
		}

		public override ISlideshowPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PhotosPresentation>();
			pres.Init(this);
			return pres;
		}
	}
}
