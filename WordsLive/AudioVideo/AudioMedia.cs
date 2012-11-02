﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.AudioVideo
{
	// TODO: in order to play Audio using WPF the presentation needs to be shown (which is just a black screen). This might be confusing.
	public class AudioMedia : AudioVideoMedia
	{
		public AudioMedia(string file, MediaDataProvider provider) : base(file, provider) { }

		public override bool HasVideo
		{
			get { return false; }
		}

		public override void Load()
		{
			// do nothing (loading is done in presentation)
		}
	}
}
