using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.Documents
{
	public class XpsDocumentHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".xps" }; }
		}

		public override string Description
		{
			get { return "XPS-Dokumente"; }
		}

		public override int Test(Uri uri)
		{
			if (CheckExtension(uri))
				return 100;

			return -1;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new XpsDocument(uri);
		}
	}
}
