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
