using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;
using Vlc.DotNet.Core;
using System.IO;

namespace Words.AudioVideo
{
	public class VideoMedia : AudioVideoMedia
	{
		public override bool HasVideo
		{
			get { return true; }
		}
	}
}
