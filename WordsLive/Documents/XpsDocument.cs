using System;
using System.IO;
using Xps = System.Windows.Xps.Packaging;

namespace WordsLive.Documents
{
	public class XpsDocument : DocumentMedia
	{
		public Xps.XpsDocument Document { get; private set; }

		public XpsDocument(Uri uri) : base(uri) { }

		public override void Load()
		{
			if (!this.Uri.IsFile)
				throw new NotImplementedException("Loading XPS document from remote URI not implemented yet.");

			Document = new Xps.XpsDocument(this.Uri.LocalPath, FileAccess.Read);
		}

		public override IDocumentPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<XpsPresentation>();
			pres.SetSourceDocument(this);
			return pres;
		}
	}
}
