using System;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public class VideoMedia : AudioVideoMedia
	{
		public VideoMedia(Uri uri) : base(uri) { }

		public override bool HasVideo
		{
			get { return true; }
		}

		public override string Title
		{
			get
			{
				string vid;
				if (IsYouTubeUri(this.Uri, out vid))
				{
					return "YouTube-Video (" + vid + ")";
				}

				return base.Title;
			}
		}

		public static bool IsYouTubeUri(Uri uri)
		{
			string tmp;
			return IsYouTubeUri(uri, out tmp);
		}

		public static bool IsYouTubeUri(Uri uri, out string videoId)
		{
			videoId = null;
			if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
			{
				/*
				 * TODO: add support for all of the following
				 * (or even more, see http://stackoverflow.com/questions/3392993/php-regex-to-get-youtube-video-id)
				 * youtube.com/v/{vidid}
				 * youtube.com/vi/{vidid}
				 * youtube.com/?v={vidid}
				 * youtube.com/?vi={vidid}
				 * youtube.com/watch?v={vidid}
				 * youtube.com/watch?vi={vidid}
				 * youtu.be/{vidid}
				 */
				if ((uri.Host == "www.youtube.com" || uri.Host == "youtube.com") && uri.AbsolutePath == "/watch")
				{
					var nvc = uri.ParseQueryString();
					videoId = nvc["v"];
					return videoId != null;
				}
				else if (uri.Host == "youtu.be")
				{
					videoId = uri.AbsolutePath.Substring(1);
					return true;
				}
			}

			return false;
		}
	}
}
