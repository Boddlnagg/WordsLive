using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive
{
	public class PdfHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new[] { ".pdf" }; }
		}

		public override string Description
		{
			get { return "PDF-Dokumente"; } // TODO: localize
		}

		public override int Test(Uri uri)
		{
			if (CheckExtension(uri))
				return 100;
			else
				return -1;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new PdfMedia(uri);
		}
	}
}
