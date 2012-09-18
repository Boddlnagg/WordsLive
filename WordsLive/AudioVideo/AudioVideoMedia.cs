using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;

namespace Words.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public abstract bool HasVideo { get; }
	}
}
