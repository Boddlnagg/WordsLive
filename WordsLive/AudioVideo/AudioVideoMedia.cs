using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public AudioVideoMedia(string file) : base(file) { }

		public abstract bool HasVideo { get; }
	}
}
