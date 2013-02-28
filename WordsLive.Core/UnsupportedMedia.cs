using System;
using WordsLive.Core.Data;

namespace WordsLive.Core
{
	public class UnsupportedMedia : Media
	{
		public UnsupportedMedia(Uri uri) : base(uri) { }

		public override string Title
		{
			get
			{
				return base.Title + " (Format wird nicht unterstützt)"; // TODO: localize (-> move labeling to UI)
			}
		}

		public override void Load()
		{
			throw new InvalidOperationException();
		}
	}
}
