using System;

namespace WordsLive.AudioVideo
{
	// TODO: in order to play Audio using WPF the presentation needs to be shown (which is just a black screen). This might be confusing.
	public class AudioMedia : AudioVideoMedia
	{
		public AudioMedia(Uri uri) : base(uri) { }

		public override bool HasVideo
		{
			get { return false; }
		}
	}
}
