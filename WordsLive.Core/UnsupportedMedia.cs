using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WordsLive.Core
{
	public class UnsupportedMedia : Media
	{
		public override string Title
		{
			get
			{
				return base.Title + " (Format wird nicht unterstützt)";
			}
		}
	}
}
