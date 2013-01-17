using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Images
{
	public class ImagesMedia : Media
	{
		public static readonly string[] ImageExtensions = new string[] { ".jpg", ".jpeg", ".png" }; // TODO: add more
		public static readonly string[] SlideshowExtensions = new string[] { ".show", /*".zip"*/ }; // TODO: enable zip

		public ObservableCollection<ImageInfo> Images { get; private set; }

		public bool CanSave { get; private set; }

		public bool CanEdit { get; private set; }

		public ImagesMedia(string file, IMediaDataProvider provider) : base(file, provider) { }

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
			//else if (ext == ".zip")
			//{
			//	Images = new ObservableCollection<ImageInfo>(LoadFromZip());
			//	CanSave = false;
			//	CanEdit = false;
			//}
			else
			{
				Images = new ObservableCollection<ImageInfo> { new ImageInfo(File, DataProvider) };
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
					// TODO: ignore non-existing file -> show error image instead in Control Panel
					if (!System.IO.File.Exists(line))
						continue;

					yield return new ImageInfo(line, DataProvider);
				}

				reader.Close();
			}
		}

		//private IEnumerable<ImageInfo> LoadFromZip()
		//{
		//	// TODO: use ZipFile.Read(stream)
		//	using (var zip = new ZipFile(this.File))
		//	{
		//		foreach (var entry in zip.Entries)
		//		{
		//			yield return new ImageInfo(entry);
		//		}
		//	}
		//}

		public void InsertImages(IEnumerable<string> paths, int index)
		{
			int i = index;
			foreach (string path in paths)
			{
				if (IsValidImageFile(path))
				{
					Images.Insert(i++, new ImageInfo(path, DataProvider));
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
					writer.WriteLine(img.File.FullName); // TODO: relative path?
				}
				writer.Close();
			}
		}

		public bool IsValidImageFile(string filename)
		{
			// TODO: support DataProvider (can't use FileInfo)
			FileInfo file = new FileInfo(filename);
			return file.Exists && ImageExtensions.Contains(file.Extension.ToLower());
		}
	}
}
