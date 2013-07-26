using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Owin.Types;
using WordsLive.Core;
using WordsLive.Core.Songs.Storage;
using WordsLive.Server.Utils;

namespace WordsLive.Server
{
	using AppFunc = Func<IDictionary<string, object>, Task>;

	public class SongsModule
	{
		private AppFunc next;

		public SongsModule(AppFunc next)
		{
			this.next = next;
		}

		public Task Invoke(IDictionary<string, object> env)
		{
			var request = new OwinRequest(env);
			var response = new OwinResponse(env);

			var requestPath = Uri.UnescapeDataString(request.Path);

			var songs = DataManager.ActualSongStorage;

			if (requestPath.StartsWith("/songs/"))
			{
				string query = requestPath.Substring("/songs/".Length);
				if (query == "list")
				{
					if (request.Method != "GET")
						return response.RespondMethodNotAllowed();

					return response.RespondCompressedString(JsonConvert.SerializeObject(songs.All()), "application/json");
				}
				else if (query == "count")
				{
					if (request.Method != "GET")
						return response.RespondMethodNotAllowed();

					return response.RespondString(songs.Count().ToString());
				}
				else if (query.StartsWith("filter/"))
				{
					if (request.Method != "GET")
						return response.RespondMethodNotAllowed();

					query = query.Substring("filter/".Length);
					var i = query.IndexOf('/');
					if (i < 0)
						return response.RespondNotFound();

					var filter = query.Substring(0, i);
					var filterQuery = SongData.NormalizeSearchString(query.Substring(i + 1));

					if (filter == "text")
					{
						return response.RespondCompressedString(JsonConvert.SerializeObject(songs.WhereTextContains(filterQuery)), "application/json");
					}
					else if (filter == "title")
					{
						return response.RespondCompressedString(JsonConvert.SerializeObject(songs.WhereTitleContains(filterQuery)), "application/json");
					}
					else if (filter == "source")
					{
						return response.RespondCompressedString(JsonConvert.SerializeObject(songs.WhereSourceContains(filterQuery)), "application/json");
					}
					else if (filter == "copyright")
					{
						return response.RespondCompressedString(JsonConvert.SerializeObject(songs.WhereCopyrightContains(filterQuery)), "application/json");
					}
					else
					{
						return response.RespondNotFound(); // unsupported filter method
					}
				}
				else
				{
					if (request.Method == "GET")
					{
						return RespondGetSong(response, songs, query);
					}
					else if (request.Method == "PUT")
					{
						return RespondPutSong(request, response, songs, query);
					}
					else if (request.Method == "DELETE")
					{
						try
						{
							songs.Delete(query);
							return TaskHelpers.Completed();
						}
						catch (FileNotFoundException)
						{
							return response.RespondNotFound();
						}
					}
					else
					{
						return response.RespondMethodNotAllowed();
					}
				}
			}
			else
			{
				return next(env);
			}
		}

		private async Task RespondGetSong(OwinResponse response, SongStorage storage, string name)
		{
			bool success = true;
			try
			{
				using (var stream = await storage.GetAsync(name, CancellationToken.None))
				{
					response.SetHeader("Content-Type", "text/xml");
					await stream.CopyToAsync(response.Body);
					// TODO: send Last-Modified header
				}
			}
			catch (FileNotFoundException)
			{
				success = false;
			}
			catch (ArgumentException)
			{
				success = false;
			}

			if (!success)
			{
				await response.RespondNotFound();
			}
		}

		private async Task RespondPutSong(OwinRequest request, OwinResponse response, SongStorage storage, string name)
		{
			var contentLength = request.Headers["Content-Length"];
			// TODO: error handling
			var ft = storage.Put(name);
			await request.Body.CopyToAsync(ft.Stream);
			await ft.FinishAsync();
		}
	}
}
