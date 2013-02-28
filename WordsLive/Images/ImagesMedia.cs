using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Ionic.Zip;
using WordsLive.Core;

namespace WordsLive.Images
{
	public class ImagesMedia : Media
	{
		public static readonly string[] ImageExtensions = new string[] { ".jpg", ".jpeg", ".png" }; // TODO: add more
		public static readonly string[] SlideshowExtensions = new string[] { ".show", ".zip" };

		public ObservableCollection<ImageInfo> Images { get; private set; }

		public bool CanSave { get; private set; }

		public bool CanEdit { get; private set; }

		public ImagesMedia(Uri uri) : base(uri) { }

		protected override void LoadMetadata()
		{
			base.LoadMetadata();
			Load(); // load slideshow stats if it is one -> able to show number of images
		}

		public override void Load()
		{
			string ext = Uri.GetExtension().ToLower();
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
				Images = new ObservableCollection<ImageInfo> { new ImageInfo(Uri) };
				CanSave = false;
				CanEdit = true;
			}

			if (!Uri.IsFile)
				CanSave = false;
		}

		internal void CreateSlideshow(IEnumerable<Uri> uris)
		{
			Images = new ObservableCollection<ImageInfo>();
			InsertImages(uris, 0);
			CanSave = true;
			CanEdit = true;
			Save();
		}

		private IEnumerable<ImageInfo> LoadFromTxt()
		{
			if (!Uri.IsFile)
				throw new NotImplementedException("Loading slideshows from a remote source is not implemented.");

			using (var reader = new StreamReader(Uri.LocalPath))
			{
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
			}
		}

		private IEnumerable<ImageInfo> LoadFromZip()
		{
			if (!Uri.IsFile)
				throw new NotImplementedException("Loading slideshows from a remote source is not implemented.");

			Stream stream = System.IO.File.OpenRead(Uri.LocalPath);
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

		public void InsertImages(IEnumerable<Uri> uris, int index)
		{
			int i = index;
			foreach (var uri in uris)
			{
				if (IsValidImageUri(uri))
				{
					Images.Insert(i++, new ImageInfo(uri));
				}
			}
		}

		public void Save()
		{
			if (!CanSave || !Uri.IsFile)
				throw new InvalidOperationException("Cannot save this ImagesMedia.");

			using (var writer = new StreamWriter(Uri.LocalPath))
			{
				foreach (var img in Images)
				{
					if (img.Uri.IsFile)
						writer.WriteLine(img.Uri.LocalPath); // TODO: support relative paths?
					else
						writer.WriteLine(img.Uri.AbsoluteUri);
				}
			}
		}

		public static bool IsValidImageUri(Uri uri)
		{
			var ext = uri.GetExtension().ToLower();
			return ImageExtensions.Contains(ext);
		}
	}
}
