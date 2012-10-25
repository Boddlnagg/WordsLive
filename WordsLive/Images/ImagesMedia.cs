using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Ionic.Zip;
using WordsLive.Core;
using System.Threading;

namespace WordsLive.Images
{
	public class ImagesMedia : Media
	{
		public static readonly string[] ImageExtensions = new string[] { ".jpg", ".jpeg", ".png" }; // TODO: add more

		public ObservableCollection<ImageInfo> Images { get; private set; }

		public bool CanSave { get; private set; }

		public bool CanEdit { get; private set; }

		public ImagesMedia(string file) : base(file) { }

		public override void Load()
		{
			FileInfo file = new FileInfo(this.File);
			if (file.Extension.ToLower() == ".show")
			{
				Images = new ObservableCollection<ImageInfo>(LoadFromTxt());
				CanSave = true;
				CanEdit = true;
			}
			else if (file.Extension.ToLower() == ".zip")
			{
				Images = new ObservableCollection<ImageInfo>(LoadFromZip());
				CanSave = false;
				CanEdit = false;
			}
			else
			{
				Images = new ObservableCollection<ImageInfo> { new ImageInfo(file) };
				CanSave = false;
				CanEdit = true;
			}
		}

		private IEnumerable<ImageInfo> LoadFromTxt()
		{
			using (StreamReader reader = new StreamReader(this.File))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (!System.IO.File.Exists(line)) // TODO: maybe show anyway? (-> loading error image)
						continue;

					yield return new ImageInfo(new FileInfo(line));
				}
			}
		}

		private IEnumerable<ImageInfo> LoadFromZip()
		{
			using (var zip = new ZipFile(this.File))
			{
				foreach (var entry in zip.Entries)
				{
					yield return new ImageInfo(entry);
				}
			}
		}

		public void Save()
		{
			var file = new FileInfo(this.File);

			if (file.Extension.ToLower() != ".show")
				throw new InvalidOperationException("Can only save txt-slideshows.");

			using (StreamWriter writer = new StreamWriter(this.File))
			{
				foreach (var img in Images)
				{
					writer.WriteLine(img.File.FullName); // TODO: relative path?
				}
			}
		}

		public bool IsValidImageFile(string filename)
		{
			FileInfo file = new FileInfo(filename);
			return file.Exists && ImageExtensions.Contains(file.Extension.ToLower());
		}
	}
}
