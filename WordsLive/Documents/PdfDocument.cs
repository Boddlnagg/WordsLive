using System;

namespace WordsLive.Documents
{
	public class PdfDocument : DocumentMedia
	{
		public PdfDocument(Uri uri) : base(uri) { }

		public override void Load()
		{
			// nothing to do
		}

		public override IDocumentPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PdfPresentation>();
			pres.Document = this;
			return pres;
		}
	}
}
