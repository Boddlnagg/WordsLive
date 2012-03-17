using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;
using Vlc.DotNet.Core;

namespace Words.AudioVideo
{
	[FileExtension(".wmv")]
	[FileExtension(".mp4")]
	[FileExtension(".avi")]
	[FileExtension(".mov")]
	public class VideoMedia : AudioVideoMedia
	{
		protected override bool LoadFromMetadata()
		{
			return true;
		}

		public override bool HasVideo
		{
			get { return true; }
		}
	}
}
