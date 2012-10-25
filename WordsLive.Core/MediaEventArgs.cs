using System;

namespace WordsLive.Core
{
	public class MediaEventArgs : EventArgs
	{
		public Media Media { get; private set; }

		public MediaEventArgs(Media media)
		{
			Media = media;
		}
	}
}
