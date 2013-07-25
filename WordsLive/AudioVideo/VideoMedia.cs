/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public class VideoMedia : AudioVideoMedia
	{
		public VideoMedia(Uri uri, Dictionary<string, string> options) : base(uri, options) { }

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

				if (this.Uri.Scheme == "dshow")
				{
					return "DirectShow-Video";
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
