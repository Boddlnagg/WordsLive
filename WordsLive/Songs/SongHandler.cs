using System.Collections.Generic;
using WordsLive.Core;
using WordsLive.Core.Songs;
using System.IO;
using WordsLive.Core.Data;
using System;

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

		public override Media Handle(Uri uri)
		{
			return new Song(uri);
		}
	}
}
