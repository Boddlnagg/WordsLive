using System.IO;
using Ionic.Zip;

namespace Words.Images
{
	public class ImageInfo
	{
		public FileInfo File { get; private set; }
		public ZipEntry ZipEntry { get; private set; }

		public object Source
		{
			get
			{
				if (File != null)
					return File;
				else
					return ZipEntry;
			}
		}

		public string Title
		{
			get
			{
				if (File != null)
					return File.Name;
				else
					return ZipEntry.FileName;
			}
		}

		public Utils.ImageLoader.SourceType SourceType
		{
			get
			{
				if (File != null)
					return Utils.ImageLoader.SourceType.LocalDisk;
				else
					return Utils.ImageLoader.SourceType.ZipFile;
			}
		}

		public ImageInfo(FileInfo file)
		{
			this.File = file;
		}

		public ImageInfo(string filename)
		{
			this.File = new FileInfo(filename);
		}

		public ImageInfo(ZipEntry entry)
		{
			this.ZipEntry = entry;
		}
	}
}
