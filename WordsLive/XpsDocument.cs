using System.IO;
using WordsLive.Core;
using WordsLive.Core.Data;
using Xps = System.Windows.Xps.Packaging;

namespace WordsLive
{
	public class XpsDocument : Media
	{
		public Xps.XpsDocument Document { get; private set; }

		public XpsDocument(string file, IMediaDataProvider provider) : base(null) { } // TODO!

		public override void Load()
		{
			Document = new Xps.XpsDocument(this.File, FileAccess.Read);
		}
	}
}
