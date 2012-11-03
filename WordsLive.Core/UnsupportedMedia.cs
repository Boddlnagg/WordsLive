using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.Core
{
	public class UnsupportedMedia : Media
	{
		public UnsupportedMedia(string file, IMediaDataProvider provider) : base(file, provider) { }

		public override string Title
		{
			get
			{
				return base.Title + " (Format wird nicht unterstützt)"; // TODO: localize
			}
		}

		public override void Load()
		{
			throw new InvalidOperationException();
		}
	}
}
