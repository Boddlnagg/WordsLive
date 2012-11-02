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
		public AudioVideoMedia(string path, MediaDataProvider provider) : base(path, provider) { }

		public abstract bool HasVideo { get; }

		public Uri MediaUri
		{
			get
			{
				return this.DataProvider.GetUri(this.File);
			}
		}

		public override void Load()
		{
			// do nothing (loading is done in presentation)
		}
	}
}
