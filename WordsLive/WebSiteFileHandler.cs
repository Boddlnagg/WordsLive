using System.Collections.Generic;
using WordsLive.Core;
using System.IO;

namespace WordsLive
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
			return new WebSite(file.FullName);
		}
	}
}
