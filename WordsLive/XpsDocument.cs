using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;
using Xps = System.Windows.Xps.Packaging;
using System.IO;

namespace Words
{
	public class XpsDocument : Media
	{
		public Xps.XpsDocument Document { get; private set; }
		public override void Load()
		{
			Document = new Xps.XpsDocument(this.File, FileAccess.Read);
			base.Load();
		}
	}
}
