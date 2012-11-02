using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public AudioVideoMedia(string file, MediaDataProvider provider) : base(file, provider) { }

		public abstract bool HasVideo { get; }
	}
}
