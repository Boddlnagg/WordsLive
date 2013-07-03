using System;
using WordsLive.Core;

namespace WordsLive.Documents
{
	public abstract class DocumentMedia : Media
	{
		public DocumentMedia(Uri uri) : base(uri) { }

		public abstract IDocumentPresentation CreatePresentation();
	}
}
