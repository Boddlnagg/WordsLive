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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Owin.Types;
using WordsLive.Core;
using WordsLive.Core.Songs.Storage;
using WordsLive.Server.Utils;

namespace WordsLive.Server
{	
	using AppFunc = Func<IDictionary<string, object>, Task>;

	public class BackgroundsModule
	{
		private AppFunc next;

		public BackgroundsModule(AppFunc next)
		{
			this.next = next;
		}

		public Task Invoke(IDictionary<string, object> env)
		{
			var request = new OwinRequest(env);
			var response = new OwinResponse(env);

			var requestPath = Uri.UnescapeDataString(request.Path);

			var backgrounds = DataManager.ActualBackgroundStorage;

			if (requestPath.StartsWith("/backgrounds/"))
			{
				if (request.Method != "GET")
					return response.RespondMethodNotAllowed();

				var query = requestPath.Substring("/backgrounds".Length);

				if (query.EndsWith("/list"))
				{
					string path = query.Substring(0, query.Length - "list".Length);
					var dir = backgrounds.GetDirectory(path);

					try
					{
						StringBuilder sb = new StringBuilder();
						ListBackgroundEntries(dir, sb);

						return response.RespondString(sb.ToString());
					}
					catch (FileNotFoundException)
					{
						return response.RespondNotFound();
					}
				}
				else if (query == "/listall")
				{
					StringBuilder sb = new StringBuilder();
					ListBackgroundEntries(backgrounds.Root, sb, true);
					return response.RespondString(sb.ToString());
				}
				else
				{
					bool preview = false;
					if (query.EndsWith("/preview"))
					{
						preview = true;
						query = query.Substring(0, query.Length - "/preview".Length);
					}

					try
					{
						var file = backgrounds.GetFile(query);
						var contentType = file.MimeType;

						if (!String.IsNullOrEmpty(contentType))
						{
							response.SetHeader("Content-Type", contentType);
						}
						return response.RespondDownloaded(preview ? file.PreviewUri : file.Uri);
					}
					catch (FileNotFoundException)
					{
						return response.RespondNotFound();
					}
				}
			}
			else
			{
				return next(env);
			}
		}

		private void ListBackgroundEntries(BackgroundDirectory parent, StringBuilder sb, bool recursive = false)
		{
			foreach (var subdir in parent.Directories)
			{
				sb.Append(subdir.Path);
				sb.Append('\n');

				if (recursive)
					ListBackgroundEntries(subdir, sb, true);
			}
			foreach (var file in parent.Files)
			{
				sb.Append(file.Path);
				sb.Append('\n');
			}
		}
	}
}
