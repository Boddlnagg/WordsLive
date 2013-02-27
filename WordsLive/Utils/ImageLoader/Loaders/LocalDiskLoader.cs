using System;
using System.IO;

namespace WordsLive.Utils.ImageLoader.Loaders
{
	internal class LocalDiskLoader: ILoader
	{
		public Stream Load(object source)
		{
			if (source is string)
				return File.OpenRead(source as string);
			else if (source is FileInfo)
				return File.OpenRead((source as FileInfo).FullName);
			else if (source is Uri)
				return File.OpenRead((source as Uri).LocalPath);
			else
				return null;
		}
	}
}
