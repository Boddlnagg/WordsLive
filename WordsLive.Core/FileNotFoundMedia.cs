using System;
using WordsLive.Core.Data;

namespace WordsLive.Core
{
	public class FileNotFoundMedia : Media
	{
		public FileNotFoundMedia(string file, IMediaDataProvider provider) : base(file, provider) { }

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
