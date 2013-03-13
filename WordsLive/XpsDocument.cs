using System;
using System.IO;
using WordsLive.Core;
using Xps = System.Windows.Xps.Packaging;

namespace WordsLive
{
	public class XpsDocument : Media
	{
		public Xps.XpsDocument Document { get; private set; }

		public XpsDocument(Uri uri) : base(uri) { }

		public override void Load()
		{
			if (!this.Uri.IsFile)
				throw new NotImplementedException("Loading XPS document from remote URI not implemented yet.");

			Document = new Xps.XpsDocument(this.Uri.LocalPath, FileAccess.Read);
		}
	}
}
