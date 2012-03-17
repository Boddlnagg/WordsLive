using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;

namespace Words.AudioVideo
{
	[FileExtension(".mp3")]
	public class AudioMedia : AudioVideoMedia
	{
		protected override bool LoadFromMetadata()
		{
			return true;
		}

		public override bool HasVideo
		{
			get { return false; }
		}
	}
}
