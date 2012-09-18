using System.Collections.Generic;
using Words.Core;
using System.IO;

namespace Words
{
	public class WebSiteFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".website", ".url" }; }
		}

		public override string Description
		{
			get { return "Webseiten"; }
		}

		public override Media TryHandle(FileInfo file)
		{
			var media = new WebSite();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
