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

			if (uri.Scheme == Uri.UriSchemeHttp)
			{
				// TODO: accept only when VLC is enabled
				if ((uri.Host == "www.youtube.com" || uri.Host == "youtube.com") && uri.AbsolutePath == "/watch" && uri.Query.StartsWith("?v="))
				{
					return 100;
				}
			}

			return -1;
		}

		public override Media Handle(Uri uri)
		{
			return new VideoMedia(uri);
		}
	}
}
