using System.Collections.Generic;
using WordsLive.Core;
using WordsLive.Core.Songs;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.Songs
{
	public class SongFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".ppl" }; }
		}

		public override string Description
		{
			get { return "Powerpraise-Lieder"; }
		}

		public override Media TryHandle(string path, IMediaDataProvider provider)
		{
			return new Song(path, provider);
		}
	}
}
