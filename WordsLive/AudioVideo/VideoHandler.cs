using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public class VideoHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".wmv", ".mp4", ".avi", ".mov" }; }
		}

		public override string Description
		{
			get { return "Video-Dateien"; } // TODO: localize
		}

		public override int Test(Uri uri)
		{
			if (CheckExtension(uri))
				return 100;

			// TODO: accept YouTube only when VLC is enabled
			if (VideoMedia.IsYouTubeUri(uri))
				return 100;

			return -1;
		}

		public override Media Handle(Uri uri)
		{
			return new VideoMedia(uri);
		}
	}
}
