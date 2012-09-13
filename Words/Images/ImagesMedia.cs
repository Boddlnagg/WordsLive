using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Words.Core;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using Ionic.Zip;

namespace Words.Images
{
	public class ImagesMedia : Media
	{
		// TODO: allow adding/reordering/removing images after loading
		public ObservableCollection<ImageInfo> Images { get; private set; }

		public override void Load()
		{
			FileInfo file = new FileInfo(this.File);
			if (file.Extension.ToLower() == ".show")
				Images = new ObservableCollection<ImageInfo>(LoadFromTxt(this.File));
			else if (file.Extension.ToLower() == ".zip")
				Images = new ObservableCollection<ImageInfo>(LoadFromZip(this.File));
			else
				Images = new ObservableCollection<ImageInfo> { new ImageInfo(file) };
		}

		private IEnumerable<ImageInfo> LoadFromTxt(string filename)
		{
			using (StreamReader reader = new StreamReader(filename))
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

		private IEnumerable<ImageInfo> LoadFromZip(string filename)
		{
			using (var zip = new ZipFile(filename))
			{
				foreach (var entry in zip.Entries)
				{
					yield return new ImageInfo(entry);
				}
			}
		}
	}
}
