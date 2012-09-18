using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;
using System.IO;

namespace Words.AudioVideo
{
	// TODO: in order to play Audio using WPF the presentation needs to be shown (which is just a black screen). This might be confusing.
	public class AudioMedia : AudioVideoMedia
	{
		public override bool HasVideo
		{
			get { return false; }
		}
	}
}
