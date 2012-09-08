using System.IO;
using Ionic.Zip;

namespace Words.ImageLoader.Loaders
{
	internal class ZipLoader: ILoader
	{
		public Stream Load(object source)
		{
			if (source is ZipEntry)
			{
				ZipEntry entry = source as ZipEntry;
				MemoryStream stream = new MemoryStream();
				entry.Extract(stream);
				stream.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
			else
			{
				return null;
			}
		}
	}
}
