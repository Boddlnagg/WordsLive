using System;

namespace WordsLive.Core
{
	public class FileNotFoundMedia : Media
	{
		public FileNotFoundMedia(Uri uri) : base(uri) { }

		public override string Title
		{
			get
			{
				return base.Title + " (Datei nicht gefunden)"; // TODO: localize (-> move labeling to UI)
			}
		}

		public override void Load()
		{
			throw new InvalidOperationException();
		}
	}
}
