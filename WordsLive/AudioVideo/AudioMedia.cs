using System;

namespace WordsLive.AudioVideo
{
	public class AudioMedia : AudioVideoMedia
	{
		public AudioMedia(Uri uri) : base(uri) { }

		public override bool HasVideo
		{
			get { return false; }
		}
	}
}
