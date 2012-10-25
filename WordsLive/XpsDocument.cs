using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core;
using Xps = System.Windows.Xps.Packaging;
using System.IO;

namespace WordsLive
{
	public class XpsDocument : Media
	{
		public Xps.XpsDocument Document { get; private set; }

		public XpsDocument(string file) : base(file) { }

		public override void Load()
		{
			Document = new Xps.XpsDocument(this.File, FileAccess.Read);
		}
	}
}
