using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;
using Vlc.DotNet.Core;

namespace Words.AudioVideo
{
	[MediaType("Video-Dateien", ".wmv", ".mp4", ".avi", ".mov")]
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
