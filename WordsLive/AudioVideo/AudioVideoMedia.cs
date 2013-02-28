using System;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public AudioVideoMedia(string path, IMediaDataProvider provider) : base(null) { } // TODO!!

		public abstract bool HasVideo { get; }

		public Uri MediaUri { get; set; }

		public override void Load()
		{
			MediaUri = this.DataProvider.GetUri(this.File);
			// TODO: load offsets from somewhere & add UI to configure it
			OffsetStart = new TimeSpan(0, 0, 0);
			OffsetEnd = new TimeSpan(0, 0, 0);
		}

		public TimeSpan OffsetStart { get; set; }

		public TimeSpan OffsetEnd { get; set; }
	}
}
