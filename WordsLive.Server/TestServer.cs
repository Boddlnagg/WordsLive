/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Firefly.Http;
using Owin;
using WordsLive.Core.Songs.Storage;
using WordsLive.Server.Utils;
using WordsLive.Server.Utils.WebSockets;

namespace WordsLive.Server
{
	using System.IO.Compression;
	using System.Net.Http;
	using System.Threading;
	using Newtonsoft.Json;
	using Owin.Builder;
	using Owin.Types;
	using WordsLive.Core;
	using AppFunc = Func<IDictionary<string, object>, Task>;

	static class Workaround
	{
		#pragma warning disable 169
		private static Action _1;
		private static Func<int, int, int> _2;
		private static Func<int, int> _3;
		#pragma warning restore 169
	}

	public class TestServer
	{
		private string HtmlContent
		{
			get
			{
				return
@"<!DOCTYPE html>
<html>
<head>
<title>WebSocket Echo Test</title>
<script type='text/javascript'>
var wsUri = 'ws://localhost:" + port + @"/Echo';
var output;

function init(){
output = document.getElementById('output');
testWebSocket();
}

function testWebSocket(){
websocket = new WebSocket(wsUri);

websocket.onopen = function(evt){
onOpen(evt)
};

websocket.onclose = function(evt){
onClose(evt)
};

websocket.onmessage = function(evt){
onMessage(evt)
};

websocket.onerror = function(evt){
onError(evt)
};
}

function onOpen(evt){
writeToScreen('CONNECTED');
doSend('WebSocket rocks');
}

function onClose(evt){
writeToScreen('DISCONNECTED');
}

function onMessage(evt){
writeToScreen('<span style=\'color: blue;\'>RESPONSE: ' + evt.data + '</span>');
websocket.close();
}

function onError(evt){
writeToScreen('<span style=\'color: red;\'>ERROR: ' + evt.data + '</span>');
}

function doSend(message){
writeToScreen('SENT: ' + message);
websocket.send(message);
}

function writeToScreen(message){
var pre = document.createElement('p');
pre.style.wordWrap = 'break-word';
pre.innerHTML = message;
output.appendChild(pre);
}

window.addEventListener('load', init, false);
</script>
</head>
<body>
<h2>WebSocket Echo Test</h2>
<div id='output'></div>
</body>
</html>";
			}
		}

		private int port;
		private string password;
		private IDisposable server;

		public bool IsRunning
		{
			get
			{
				return server != null;
			}
		}

		public int Port
		{
			get
			{
				return port;
			}
			set
			{
				if (IsRunning)
					throw new InvalidOperationException("Server already running.");

				port = value;
			}
		}

		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				if (IsRunning)
					throw new InvalidOperationException("Server already running.");

				password = value;
			}
		}

		public TestServer(int port)
		{
			this.port = port;
		}

		/// <summary>
		/// Creates a song storage that uses this server as a backend.
		/// </summary>
		/// <returns>The created storage.</returns>
		public SongStorage CreateSongStorage()
		{
			NetworkCredential cred = null;

			if (!String.IsNullOrEmpty(Password))
				cred = new NetworkCredential("WordsLive", Password);

			return new HttpSongStorage("http://localhost:" + Port + "/songs/", cred); // alternative: ipv4.fiddler
		}

		public BackgroundStorage CreateBackgroundStorage()
		{
			NetworkCredential cred = null;

			if (!String.IsNullOrEmpty(Password))
				cred = new NetworkCredential("WordsLive", Password);

			return new HttpBackgroundStorage("http://localhost:" + Port + "/backgrounds/", cred); // alternative: ipv4.fiddler
		}

		private static AppFunc EnableAuthentication(AppFunc app, string password)
		{
			return DigestAuthentication.Enable(
				app,
				(env) => 
					{
						var request = new OwinRequest(env);

						if (request.CanAccept)
							return false;

						if (!request.Path.StartsWith("/backgrounds/"))
							return true;

						if (request.Path.EndsWith("/list") || request.Path.EndsWith("/listall"))
							return true;

						return false; // all other requests to /backgrounds/ without /list(all)
					},
				"WordsLive",
				(user) => (user == "WordsLive" ?
					new DigestAuthentication.UserPassword { Password = password } :
					(DigestAuthentication.UserPassword?)null)
			);
		}

		private AppFunc WebSocketHandler(AppFunc next)
		{
			return (env) =>
			{
				var request = new OwinRequest(env);

				if (request.Path == "/Echo" && request.CanAccept)
				{
					request.Accept(async (socket) =>
					{
						const int maxMessageSize = 1024;
						byte[] receiveBuffer = new byte[maxMessageSize];

						Console.WriteLine("WebSocket connection established.");

						while (true)
						{
							ArraySegment<byte> buffer = new ArraySegment<byte>(receiveBuffer);
							var received = await socket.ReceiveAsync(buffer, CancellationToken.None);
							if (received.MessageType == 0x8) // close
							{
								await socket.CloseAsync(CancellationToken.None);
								return;
							}
							else if (received.MessageType == 0x2) // binary
							{
								await socket.CloseAsync((int)WebSocketCloseStatus.InvalidMessageType, "Cannot accept binary frame", CancellationToken.None);
							}
							else
							{
								int count = received.Count;

								while (received.EndOfMessage == false)
								{
									if (count >= maxMessageSize)
									{
										throw new NotSupportedException();
									}

									received = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, count, maxMessageSize - count), CancellationToken.None);
									count += received.Count;
								}

								var receivedString = Encoding.UTF8.GetString(receiveBuffer, 0, count);
								var echoString = receivedString;
								ArraySegment<byte> outputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(echoString));
								await socket.SendAsync(outputBuffer, (int)WebSocketMessageType.Text, true, CancellationToken.None);
							}
						}
					});
					return TaskHelpers.Completed();
				}
				else if (request.Path == "/")
				{
					var response = new OwinResponse(env);
					return RespondString(response, HtmlContent, "text/html");
				}
				else
				{
					return next(env);
				}
			};
		}

		/// <summary>
		/// Starts this server.
		/// </summary>
		public void Start()
		{
			var fac = new ServerFactory();

			var builder = new AppBuilder();
			builder.UseWebSockets();

			if (!String.IsNullOrEmpty(Password))
			{
				builder.UseFunc(EnableAuthentication, Password);
			}

			builder.UseFunc(WebSocketHandler);
			builder.UseFunc(BackgroundsHandler);
			builder.UseFunc(SongsHandler);

			var app = Owin.StartupExtensions.Build<AppFunc>(builder);

			server = fac.Create(app, this.Port);
		}

		/// <summary>
		/// Stops this server.
		/// </summary>
		public void Stop()
		{
			if (!IsRunning)
				throw new InvalidOperationException("Not running.");

			server.Dispose();
			server = null;
		}

		private AppFunc BackgroundsHandler(AppFunc next)
		{
			return (env) =>
			{
				var request = new OwinRequest(env);
				var response = new OwinResponse(env);

				var requestPath = Uri.UnescapeDataString(request.Path);

				var backgrounds = DataManager.ActualBackgroundStorage;

				if (requestPath.StartsWith("/backgrounds/"))
				{
					if (request.Method != "GET")
						return RespondMethodNotAllowed(response);

					var query = requestPath.Substring("/backgrounds".Length);

					if (query.EndsWith("/list"))
					{
						string path = query.Substring(0, query.Length - "list".Length);
						var dir = backgrounds.GetDirectory(path);

						try
						{
							StringBuilder sb = new StringBuilder();
							ListBackgroundEntries(dir, sb);

							return RespondString(response, sb.ToString());
						}
						catch (FileNotFoundException)
						{
							return RespondNotFound(response);
						}
					}
					else if (query == "/listall")
					{
						StringBuilder sb = new StringBuilder();
						ListBackgroundEntries(backgrounds.Root, sb, true);
						return RespondString(response, sb.ToString());
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
							return RespondDownloaded(response, preview ? file.PreviewUri : file.Uri);
						}
						catch (FileNotFoundException)
						{
							return RespondNotFound(response);
						}
					}
				}
				else
				{
					return next(env);
				}
			};
		}

		private AppFunc SongsHandler(AppFunc next)
		{
			return (env) =>
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
							return RespondMethodNotAllowed(response);

						return RespondCompressedString(response, JsonConvert.SerializeObject(songs.All()), "application/json");
					}
					else if (query == "count")
					{
						if (request.Method != "GET")
							return RespondMethodNotAllowed(response);

						return RespondString(response, songs.Count().ToString());
					}
					else if (query.StartsWith("filter/"))
					{
						if (request.Method != "GET")
							return RespondMethodNotAllowed(response);

						query = query.Substring("filter/".Length);
						var i = query.IndexOf('/');
						if (i < 0)
							return RespondNotFound(response);

						var filter = query.Substring(0, i);
						var filterQuery = SongData.NormalizeSearchString(query.Substring(i + 1));

						if (filter == "text")
						{
							return RespondCompressedString(response, JsonConvert.SerializeObject(songs.WhereTextContains(filterQuery)), "application/json");
						}
						else if (filter == "title")
						{
							return RespondCompressedString(response, JsonConvert.SerializeObject(songs.WhereTitleContains(filterQuery)), "application/json");
						}
						else if (filter == "source")
						{
							return RespondCompressedString(response, JsonConvert.SerializeObject(songs.WhereSourceContains(filterQuery)), "application/json");
						}
						else if (filter == "copyright")
						{
							return RespondCompressedString(response, JsonConvert.SerializeObject(songs.WhereCopyrightContains(filterQuery)), "application/json");
						}
						else
						{
							return RespondNotFound(response); // unsupported filter method
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
								return RespondNotFound(response);
							}
						}
						else
						{
							return RespondMethodNotAllowed(response);
						}
					}
				}
				else
				{
					return next(env);
				}
			};
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
				await RespondNotFound(response);
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

		private async Task RespondDownloaded(OwinResponse response, Uri uri)
		{
			if (uri.IsFile)
			{
				using (var stream = File.OpenRead(uri.LocalPath))
				{
					await stream.CopyToAsync(response.Body).ConfigureAwait(false);
				}
			}
			else
			{
				using (HttpClient client = new HttpClient())
				{
					// TODO: set content type
					var body = await client.GetStreamAsync(uri).ConfigureAwait(false);
					await body.CopyToAsync(response.Body).ConfigureAwait(false);
				}
			}
		}

		private Task RespondString(OwinResponse response, string content, string contentType = "text/plain")
		{
			response.SetHeader("Content-Type", contentType + ";charset=utf-8");
			return response.WriteAsync(content);
		}

		private async Task RespondCompressedString(OwinResponse response, string content, string contentType = "text/plain")
		{
			response.SetHeader("Content-Type", contentType + ";charset=utf-8");
			response.SetHeader("Content-Encoding", "gzip");
			using (var inStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				using (GZipStream compressedStream = new GZipStream(response.Body, CompressionMode.Compress, true))
				{
					await inStream.CopyToAsync(compressedStream);
				}
			}
		}

		private Task RespondNotFound(OwinResponse response)
		{
			response.StatusCode = 404;
			response.ReasonPhrase = "Not Found";
			return response.WriteAsync("Not Found");
		}

		private Task RespondMethodNotAllowed(OwinResponse response)
		{
			response.StatusCode = 405;
			response.ReasonPhrase = "Method Not Allowed";
			return response.WriteAsync("Request method not allowed.");
		}

		private void ListBackgroundEntries(BackgroundDirectory parent, StringBuilder sb, bool recursive = false)
		{
			foreach (var subdir in parent.Directories)
			{
				sb.Append(subdir.Path);
				sb.Append('\n');

				if (recursive)
					ListBackgroundEntries(subdir, sb);
			}
			foreach (var file in parent.Files)
			{
				sb.Append(file.Path);
				if (file.IsVideo)
					sb.Append(" [VIDEO]");
				sb.Append('\n');
			}
		}
	}
}
