using System.Collections.Generic;
using System.IO;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.AudioVideo
{
	public class AudioFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".mp3", ".wav" }; }
		}

		public override string Description
		{
			get { return "Audio-Dateien"; } // TODO: localize
		}

		public override Media TryHandle(string path, MediaDataProvider provider)
		{
			return new AudioMedia(path, provider);
		}
	}
}
