using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Ionic.Zip;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Images
{
	public class ImagesMedia : Media
	{
		public static readonly string[] ImageExtensions = new string[] { ".jpg", ".jpeg", ".png" }; // TODO: add more
		public static readonly string[] SlideshowExtensions = new string[] { ".show", ".zip" };

		public ObservableCollection<ImageInfo> Images { get; private set; }

		public bool CanSave { get; private set; }

		public bool CanEdit { get; private set; }

		public ImagesMedia(string file, IMediaDataProvider provider) : base(null) { } // TODO!!

		protected override void LoadMetadata()
		{
			base.LoadMetadata();
			Load(); // load slideshow stats if it is one -> able to show number of images
		}

		public override void Load()
		{
			string ext = Path.GetExtension(File).ToLower();
			if (ext == ".show")
			{
				Images = new ObservableCollection<ImageInfo>(LoadFromTxt());
				CanSave = true;
				CanEdit = true;
			}
			else if (ext == ".zip")
			{
				Images = new ObservableCollection<ImageInfo>(LoadFromZip());
				CanSave = false;
				CanEdit = false;
			}
			else
			{
				Images = new ObservableCollection<ImageInfo> { new ImageInfo(new Uri(File)) }; // TODO: different providers?
				CanSave = false;
				CanEdit = true;
			}

			if (!(DataProvider is IBidirectionalMediaDataProvider))
				CanSave = false;
		}

		internal void CreateSlideshow(IEnumerable<string> files)
		{
			Images = new ObservableCollection<ImageInfo>();
			InsertImages(files, 0);
			CanSave = true;
			CanEdit = true;
			Save();
		}

		private IEnumerable<ImageInfo> LoadFromTxt()
		{
			using (var stream = DataProvider.Get(File))
			{
				var reader = new StreamReader(stream);
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					ImageInfo next = null;
					try
					{
						next = new ImageInfo(new Uri(line));
					}
					catch (UriFormatException)
					{
						continue;
					}

					yield return next;
				}

				reader.Close();
			}
		}

		private IEnumerable<ImageInfo> LoadFromZip()
		{
			Stream stream = DataProvider.Get(File);
			using (var zip = ZipFile.Read(stream))
			{
				foreach (var entry in zip.Entries)
				{
					yield return new ImageInfo(entry);
				}
			}

			// important: don't close stream directly, so ImageLoader can load the images
			// TODO: close & dispose the stream when it isn't needed anymore
		}

		public void InsertImages(IEnumerable<string> paths, int index)
		{
			int i = index;
			foreach (var path in paths)
			{
				var uri = new Uri(path);
				if (IsValidImageUri(uri))
				{
					Images.Insert(i++, new ImageInfo(uri));
				}
			}
		}

		public void Save()
		{
			if (!CanSave)
				throw new InvalidOperationException("Cannot save this ImagesMedia.");

			using (var trans = (DataProvider as IBidirectionalMediaDataProvider).Put(File))
			{
				var writer = new StreamWriter(trans.Stream);
				foreach (var img in Images)
				{
					if (img.Uri.IsFile)
						writer.WriteLine(img.Uri.LocalPath); // TODO: support relative paths?
					else
						writer.WriteLine(img.Uri.AbsoluteUri);
				}
				writer.Close();
			}
		}

		public bool IsValidImageUri(Uri uri)
		{
			var ext = uri.GetExtension();
			return ImageExtensions.Contains(ext);
		}
	}
}
