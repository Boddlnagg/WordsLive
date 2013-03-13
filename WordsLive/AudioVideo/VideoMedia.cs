using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;
using Vlc.DotNet.Core;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.AudioVideo
{
	public class VideoMedia : AudioVideoMedia
	{
		public VideoMedia(Uri uri) : base(uri) { }

		public override bool HasVideo
		{
			get { return true; }
		}
	}
}
