using System.Collections.Generic;
using System.IO;
using Words.Core;

namespace Words.AudioVideo
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
			var media = new VideoMedia();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
