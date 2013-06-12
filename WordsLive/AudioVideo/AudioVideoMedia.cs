using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public AudioVideoMedia(Uri uri, Dictionary<string, string> options) : base(uri, options) { }

		public abstract bool HasVideo { get; }

		public override void Load()
		{
			offsetStart = Options.ContainsKey("start") ? new TimeSpan(0, 0, 0, 0, int.Parse(Options["start"])) : new TimeSpan(0);
			offsetEnd = Options.ContainsKey("end") ? new TimeSpan(0, 0, 0, 0, int.Parse(Options["end"])) : new TimeSpan(0);
		}

		private TimeSpan offsetStart;
		private TimeSpan offsetEnd;

		public TimeSpan OffsetStart
		{
			get
			{
				return offsetStart;
			}
			set
			{
				offsetStart = value;
				Options["start"] = ((int)offsetStart.TotalMilliseconds).ToString();
			}
		}

		public TimeSpan OffsetEnd
		{
			get
			{
				return offsetEnd;
			}
			set
			{
				offsetEnd = value;
				Options["end"] = ((int)offsetEnd.TotalMilliseconds).ToString();
			}
		}
	}
}
