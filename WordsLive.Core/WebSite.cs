using System;
using System.IO;

namespace WordsLive.Core
{
	public class WebSite : Media
	{
		public WebSite(Uri uri) : base(uri) { }

		public string Url { get; private set; }

		public override string Title
		{
			get
			{
				string f = base.Title;
				return f.Substring(0, f.LastIndexOf('.'));
			}
		}

		public override void Load()
		{
			if (this.Uri.IsFile)
			{
				using (StreamReader reader = new StreamReader(this.Uri.LocalPath))
				{
					while (!reader.EndOfStream && reader.ReadLine() != "[InternetShortcut]") { }

					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						if (line.StartsWith("URL="))
						{
							Url = line.Substring(4);
							return;
						}
					}
				}
			}
			else
			{
				throw new NotImplementedException("Loading remote websites not yet implemented.");
			}
		}
	}
}
