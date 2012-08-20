using System.Collections.Generic;
using System.IO;
using Words.Core;

namespace Words.AudioVideo
{
	public class AudioFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".mp3", ".wav" }; }
		}

		public override string Description
		{
			get { return "Audio-Dateien"; }
		}

		public override Media TryHandle(FileInfo file)
		{
			var media = new AudioMedia();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
