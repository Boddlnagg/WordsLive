using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Words.Core;

namespace Words.Slideshow.Photos
{
	[MediaType("Diashows", ".show")]
	public class PhotosMedia : SlideshowMedia
	{
		public class PhotoInfo
		{
			private const string orientationQuery = "System.Photo.Orientation";
			private const int thumbnailHeight = 120;

			public ImageSource Thumbnail { get; private set; }
			public string File { get; private set; }
			public Rotation Rotation { get; private set; }

			private BitmapImage cachedFullImage;

			public PhotoInfo(string file)
			{
				File = file;
				Thumbnail = Load(true);
			}

			private BitmapImage Load(bool thumbnail)
			{
				BitmapImage img = new BitmapImage();

				Rotation = Rotation.Rotate0;
				using (FileStream stream = new FileStream(File, FileMode.Open, FileAccess.Read))
				{
					BitmapFrame bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
					BitmapMetadata bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;

					if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(orientationQuery)))
					{
						object o = bitmapMetadata.GetQuery(orientationQuery);

						if (o != null)
						{
							//refer to http://www.impulseadventure.com/photo/exif-orientation.html for details on orientation values
							switch ((ushort)o)
							{
								case 6:
									Rotation = Rotation.Rotate90;
									break;
								case 3:
									Rotation = Rotation.Rotate180;
									break;
								case 8:
									Rotation = Rotation.Rotate270;
									break;
							}
						}
					}
				}

				img.BeginInit();
				if (thumbnail)
				{
					if (Rotation == Rotation.Rotate0 || Rotation == Rotation.Rotate180)
						img.DecodePixelHeight = thumbnailHeight;
					else
						img.DecodePixelWidth = thumbnailHeight;
				}
				img.Rotation = Rotation;
				img.CacheOption = BitmapCacheOption.OnLoad;
				img.UriSource = new Uri(File);
				img.EndInit();
				return img;
			}

			public ImageSource LoadImage()
			{
				if (cachedFullImage == null)
				{
					cachedFullImage = Load(false);
					cachedFullImage.Freeze();
				}
				return cachedFullImage;
			}

			public void PreCache()
			{
				if (cachedFullImage == null)
					ThreadPool.QueueUserWorkItem((o) =>
					{
						cachedFullImage = Load(false);
						cachedFullImage.Freeze();
					});
			}
		}

		public IList<ImageSource> Thumbnails { get; private set; }
		public IEnumerable<PhotoInfo> Photos { get; private set; }

		protected override bool LoadFromMetadata()
		{
			Photos = LoadFromTxt(this.File);

			Thumbnails = new List<ImageSource>();
			foreach (var photo in Photos)
			{
				Thumbnails.Add(photo.Thumbnail);
			}
			return true;
		}

		private IEnumerable<PhotoInfo> LoadFromTxt(string filename)
		{
			using (StreamReader reader = new StreamReader(filename))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (!System.IO.File.Exists(line))
						continue;

					yield return new PhotoInfo(line);
				}
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
