using System;
using System.Collections.Generic;

namespace WordsLive.AudioVideo
{
	public class AudioMedia : AudioVideoMedia
	{
		public AudioMedia(Uri uri, Dictionary<string, string> options) : base(uri, options) { }

		public override bool HasVideo
		{
			get { return false; }
		}
	}
}
