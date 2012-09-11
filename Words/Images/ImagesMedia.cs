using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Words.Core;
using System.Linq;
using System.Windows;

namespace Words.Images
{
	public class ImagesMedia : Media
	{
		// TODO: allow adding/reordering/removing images after loading

		public IList<FileInfo> Images { get; private set; }

		public override void Load()
		{
			FileInfo file = new FileInfo(this.File);
			if (file.Extension.ToLower() == ".show")
				Images = LoadFromTxt(this.File).ToList();
			else
				Images = new List<FileInfo> { file };
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
