using System.Collections.Generic;
using WordsLive.Core;
using WordsLive.Core.Songs;
using System.IO;

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

		public override Media TryHandle(FileInfo file)
		{
			var media = new Song();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
