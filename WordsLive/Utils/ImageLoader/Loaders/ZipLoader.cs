using System;
using System.IO;
using Ionic.Zip;

namespace WordsLive.Utils.ImageLoader.Loaders
{
	internal class ZipLoader: ILoader
	{
		private readonly static object Lock = new Object();

		public Stream Load(object source)
		{
			if (source is ZipEntry)
			{
				ZipEntry entry = source as ZipEntry;
				lock(Lock)
				{
					MemoryStream stream = new MemoryStream();
					entry.Extract(stream);
					stream.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					return stream;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
