/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

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
