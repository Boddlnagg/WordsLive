using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WordsLive.Core
{
	public class FileNotFoundMedia : Media
	{
		public override string Title
		{
			get
			{
				return base.Title + " (Datei nicht gefunden)";
			}
		}
	}
}
