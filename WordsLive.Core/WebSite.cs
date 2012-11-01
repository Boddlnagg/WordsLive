﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WordsLive.Core
{
	public class WebSite : Media
	{
		public WebSite(string file) : base(file) { }

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
			using (StreamReader reader = new StreamReader(this.File))
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
	}
}