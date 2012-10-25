using System.Collections.Generic;
using System.IO;
using WordsLive.Core;

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
			get { return "Video-Dateien"; }
		}

		public override Media TryHandle(FileInfo file)
		{
			return new VideoMedia(file.FullName);
		}
	}
}
