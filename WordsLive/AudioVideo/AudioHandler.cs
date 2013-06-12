using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public class AudioHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".mp3", ".wav", ".ogg" }; }
			// TODO: ogg only supported by VLC -> allow more formats if VLC is enabled
		}

		public override string Description
		{
			get { return "Audio-Dateien"; } // TODO: localize
		}

		public override int Test(Uri uri)
		{
			return CheckExtension(uri) ? 100 : -1;
		}

		public override Media Handle(Uri uri)
		{
			return new AudioMedia(uri);
		}
	}
}
