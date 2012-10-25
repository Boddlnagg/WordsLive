using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WordsLive.Core
{
	public class FileNotFoundMedia : Media
	{
		public FileNotFoundMedia(string file) : base(file) { }

		public override string Title
		{
			get
			{
				return base.Title + " (Datei nicht gefunden)";
			}
		}

		public override void Load()
		{
			throw new InvalidOperationException();
		}
	}
}
