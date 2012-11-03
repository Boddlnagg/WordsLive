using System.Collections.Generic;
using System.IO;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.AudioVideo
{
	public class VideoFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".wmv", ".mp4", ".avi", ".mov" }; }
		}

		public override string Description
		{
			get { return "Video-Dateien"; } // TODO: localize
		}

		public override Media TryHandle(string path, IMediaDataProvider provider)
		{
			return new VideoMedia(path, provider);
		}
	}
}
