using System;

namespace WordsLive.AudioVideo
{
	public class VideoMedia : AudioVideoMedia
	{
		public VideoMedia(Uri uri) : base(uri) { }

		public override bool HasVideo
		{
			get { return true; }
		}
	}
}
