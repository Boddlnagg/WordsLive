using System;

namespace WordsLive.Utils.ImageLoader.Loaders
{
	internal static class LoaderFactory
	{
		public static ILoader CreateLoader(SourceType sourceType)
		{
			switch (sourceType)
			{
				case SourceType.LocalDisk:
					return new LocalDiskLoader();
				case SourceType.ExternalResource:
					return new ExternalLoader();
				case SourceType.ZipFile:
					return new ZipLoader();
				default:
					throw new ApplicationException("Unexpected exception");
			}
		}
	}
}
