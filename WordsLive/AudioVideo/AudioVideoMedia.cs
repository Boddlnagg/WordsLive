using System;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public AudioVideoMedia(Uri uri) : base(uri) { }

		public abstract bool HasVideo { get; }

		public override void Load()
		{
			// TODO: load offsets from somewhere & add UI to configure it
			OffsetStart = new TimeSpan(0, 0, 0);
			OffsetEnd = new TimeSpan(0, 0, 0);
		}

		public TimeSpan OffsetStart { get; set; }

		public TimeSpan OffsetEnd { get; set; }
	}
}
