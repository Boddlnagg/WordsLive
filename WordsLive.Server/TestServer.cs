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
using System.Linq;
using System.Net;
using System.Text;
using Firefly.Http;
using Newtonsoft.Json;
using Owin;
using WordsLive.Core.Data;
using WordsLive.Server.Utils;

namespace WordsLive.Server
{
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
<p><i>Requested: ###</i></p>
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
		/// Creates a data provider that uses this server as a backend.
		/// </summary>
		/// <returns>The created provider.</returns>
		public SongDataProvider CreateSongDataProvider()
		{
			NetworkCredential cred = null;

			if (!String.IsNullOrEmpty(Password))
				cred = new NetworkCredential("WordsLive", Password);

			return new HttpSongDataProvider("http://localhost:"+Port+"/songs/", cred);
		}

		public BackgroundDataProvider CreateBackgroundDataProvider()
		{
			NetworkCredential cred = null;

			if (!String.IsNullOrEmpty(Password))
				cred = new NetworkCredential("WordsLive", Password);

			return new HttpBackgroundDataProvider("http://localhost:"+Port+"/backgrounds/", cred);
		}

		private static AppDelegate EnableAuthentication(AppDelegate app, string password)
		{
			return DigestAuthentication.Enable(
				app,
				(env) => 
					{
						var requestPath = (string)env["owin.RequestPath"];
						if (!requestPath.StartsWith("/backgrounds/"))
							return true;

						if (requestPath.EndsWith("/list") || requestPath.EndsWith("/listall"))
							return true;

						return false; // all other requests to /backgrounds/ without /list(all)
					},
				"WordsLive",
				(user) => (user == "WordsLive" ?
					new DigestAuthentication.UserPassword { Password = password } :
					(DigestAuthentication.UserPassword?)null)
			);
		}

		/// <summary>
		/// Starts this server.
		/// </summary>
		public void Start()
		{
			var fac = new ServerFactory();
			AppDelegate app = App;

			if (!String.IsNullOrEmpty(Password))
			{
				app = EnableAuthentication(app, Password);
			}

			server = fac.Create(WebSockets.Enable(app, "/Echo", OnWebSocketConnection), this.Port);
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

		private Action<int, ArraySegment<byte>> OnWebSocketConnection(Action<int, ArraySegment<byte>> outgoing)
		{
			Console.WriteLine("<-> Connected");
			//outgoing(1, new ArraySegment<byte>(Encoding.Default.GetBytes("Good morning!")));
			return
				(opcode, data) =>
				{
					Console.WriteLine(" -> Incoming opcode: {0}", opcode);
					switch (opcode)
					{
						case 1:
							var prev = Console.ForegroundColor;
							Console.ForegroundColor = ConsoleColor.Blue;
							Console.WriteLine(Encoding.Default.GetString(data.Array, data.Offset, data.Count));
							Console.ForegroundColor = prev;
							break;
					}
					outgoing(opcode, data);
				};
		}

		private void App(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
		{
			string requestPath = Uri.UnescapeDataString((string)env["owin.RequestPath"]);
			string requestMethod = (string)env["owin.RequestMethod"];

			var songs = DataManager.ActualSongDataProvider;
			var backgrounds = DataManager.ActualBackgroundDataProvider;

			if (requestPath.StartsWith("/backgrounds/"))
			{
				if (requestMethod != "GET")
					RespondMethodNotAllowed(result);

				var query = requestPath.Substring("/backgrounds".Length);
				
				if (query.EndsWith("/list"))
				{
					string path = query.Substring(0, query.Length - "list".Length);
					var dir = backgrounds.GetDirectory(path);

					StringBuilder sb = new StringBuilder();
					ListBackgroundEntries(dir, sb);

					Respond(result, sb.ToString());
				}
				else if (query == "/listall")
				{
					StringBuilder sb = new StringBuilder();
					ListBackgroundEntries(backgrounds.Root, sb, true);
					Respond(result, sb.ToString());
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
						using (WebClient client = new WebClient())
						{
							var bytes = client.DownloadData(preview ? file.PreviewUri : file.Uri);
							// TODO: the Content-Type is always octet-stream if using local files. Is that a problem?
							var contentType = client.ResponseHeaders["Content-Type"];
							Respond(result, bytes, contentType: contentType);
						}
					}
					catch (FileNotFoundException)
					{
						RespondNotFound(result);
					}
				}
			}
			else if (requestPath.StartsWith("/songs/"))
			{
				string query = requestPath.Substring("/songs/".Length);
				if (query == "list")
				{
					if (requestMethod != "GET")
						RespondMethodNotAllowed(result);

					Respond(result, JsonConvert.SerializeObject(songs.All()));
				}
				else if (query == "count")
				{
					if (requestMethod != "GET")
						RespondMethodNotAllowed(result);

					Respond(result, songs.Count().ToString());
				}
				else
				{
					if (requestMethod == "GET")
					{
						try
						{
							using (var stream = songs.Get(query))
							{
								// TODO: send Last-Modified header
								Respond(result, ReadStream(stream), contentType: "text/xml");
							}
						}
						catch (FileNotFoundException)
						{
							RespondNotFound(result);
						}
					}
					else if (requestMethod == "PUT")
					{
						var contentLength = int.Parse(((IDictionary<string, IEnumerable<string>>)env["owin.RequestHeaders"])["Content-Length"].Single());
						var requestBody = (BodyDelegate)env["owin.RequestBody"];

						var responseBody = Extensions.BufferedRequestBody(requestBody, contentLength, (bytes) =>
							{
								using (var ft = songs.Put(query))
								{
									ft.Stream.Write(bytes, 0, bytes.Length);
								}
							});

						result(
							"200 OK",
							new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
						{
							{"Content-Type", new[] {"text/plain"}},
						},
							responseBody
						);
					}
					else if (requestMethod == "DELETE")
					{
						try
						{
							songs.Delete(query);
							Respond(result, "OK");
						}
						catch (FileNotFoundException)
						{
							RespondNotFound(result);
						}
					}
				}
			}
			else
			{
				Respond(result, HtmlContent.Replace("###", requestPath), contentType: "text/html");
			}
		}

		private void Respond(ResultDelegate del, string response, string contentType = "text/plain", string code = "200 OK")
		{
			Respond(del, Encoding.Default.GetBytes(response), contentType, code);
		}

		private void Respond(ResultDelegate del, byte[] response, string contentType = "text/plain", string code = "200 OK")
		{
			del(
				code,
				new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
				{
					{"Content-Type", new[] { contentType }}
				},
				(write, flush, end, cancel) =>
				{
					write(new ArraySegment<byte>(response));
					end(null);
				}
			);
		}

		private void RespondNotFound(ResultDelegate del)
		{
			Respond(del, "Not Found", code: "404 Not Found");
		}

		private void RespondMethodNotAllowed(ResultDelegate del)
		{
			Respond(del, "Request method not allowed.", code: "405 Method Not Allowed");
		}

		private byte[] ReadStream(Stream stream)
		{
			byte[] bytes;

			if (stream is MemoryStream)
			{
				bytes = (stream as MemoryStream).ToArray();
			}
			else
			{
				using (MemoryStream ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					bytes = ms.ToArray();
				}
			}

			return bytes;
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
