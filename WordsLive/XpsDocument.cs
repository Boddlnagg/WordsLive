using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;
using Xps = System.Windows.Xps.Packaging;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive
{
	public class XpsDocument : Media
	{
		public Xps.XpsDocument Document { get; private set; }

		public XpsDocument(string file, IMediaDataProvider provider) : base(file, provider) { }

		public override void Load()
		{
			Document = new Xps.XpsDocument(this.File, FileAccess.Read);
		}
	}
}
