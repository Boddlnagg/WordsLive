using System.Collections.Generic;
using Words.Core;
using System.IO;

namespace Words
{
	public class XpsDocumentFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".xps" }; }
		}

		public override string Description
		{
			get { return "XPS-Dokumente"; }
		}

		public override Media TryHandle(FileInfo file)
		{
			var media = new XpsDocument();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
