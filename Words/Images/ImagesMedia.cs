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

namespace Words.Images
{
	public class ImagesMedia : Media
	{
		// TODO: allow adding/reordering/removing images after loading

		public ObservableCollection<FileInfo> Images { get; private set; }

		public override void Load()
		{
			FileInfo file = new FileInfo(this.File);
			if (file.Extension.ToLower() == ".show")
				Images = new ObservableCollection<FileInfo>(LoadFromTxt(this.File).ToList());
			else
				Images = new ObservableCollection<FileInfo> { file };
		}

		private IEnumerable<FileInfo> LoadFromTxt(string filename)
		{
			using (StreamReader reader = new StreamReader(filename))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (!System.IO.File.Exists(line))
						continue;

					yield return new FileInfo(line);
				}
			}
		}
	}
}
