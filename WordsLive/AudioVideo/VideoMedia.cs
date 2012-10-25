using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;
using Vlc.DotNet.Core;
using System.IO;

namespace WordsLive.AudioVideo
{
	public class VideoMedia : AudioVideoMedia
	{
		public VideoMedia(string file) : base(file) { }

		public override bool HasVideo
		{
			get { return true; }
		}

		public override void Load()
		{
			// do nothing (loading is done in presentation)
		}
	}
}
