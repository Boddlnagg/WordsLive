using System;
using System.Collections.Generic;
using WordsLive.Core;
using WordsLive.Core.Songs;

namespace WordsLive.Songs
{
	public class SongHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".ppl" }; }
		}

		public override string Description
		{
			get { return "Powerpraise-Lieder"; } // TODO: localize
		}

		public override int Test(Uri uri)
		{
			return CheckExtension(uri) ? 100 : -1;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new SongMedia(DataManager.Songs.TryRewriteUri(uri));
		}
	}
}
