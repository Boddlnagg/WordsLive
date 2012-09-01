using System;

namespace Words.PhotoLoader.ImageLoaders
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
				default:
					throw new ApplicationException("Unexpected exception");
			}
		}
	}
}
