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
	using System.Threading;
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
					return response.WriteAsync(HtmlContent);
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

		//private void App(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
		//{
		//	string requestPath = Uri.UnescapeDataString((string)env["owin.RequestPath"]);
		//	string requestMethod = (string)env["owin.RequestMethod"];

		//	var songs = DataManager.ActualSongStorage;
		//	var backgrounds = DataManager.ActualBackgroundStorage;

		//	if (requestPath.StartsWith("/backgrounds/"))
		//	{
		//		if (requestMethod != "GET")
		//			RespondMethodNotAllowed(result);

		//		var query = requestPath.Substring("/backgrounds".Length);
				
		//		if (query.EndsWith("/list"))
		//		{
		//			string path = query.Substring(0, query.Length - "list".Length);
		//			var dir = backgrounds.GetDirectory(path);

		//			try
		//			{
		//				StringBuilder sb = new StringBuilder();
		//				ListBackgroundEntries(dir, sb);

		//				Respond(result, sb.ToString());
		//			}
		//			catch (FileNotFoundException)
		//			{
		//				RespondNotFound(result);
		//			}
		//		}
		//		else if (query == "/listall")
		//		{
		//			StringBuilder sb = new StringBuilder();
		//			ListBackgroundEntries(backgrounds.Root, sb, true);
		//			Respond(result, sb.ToString());
		//		}
		//		else
		//		{
		//			bool preview = false;
		//			if (query.EndsWith("/preview"))
		//			{
		//				preview = true;
		//				query = query.Substring(0, query.Length - "/preview".Length);
		//			}

		//			try
		//			{
		//				var file = backgrounds.GetFile(query);
		//				using (WebClient client = new WebClient())
		//				{
		//					var bytes = client.DownloadData(preview ? file.PreviewUri : file.Uri);
		//					// TODO: the Content-Type is always octet-stream if using local files. Is that a problem?
		//					var contentType = client.ResponseHeaders["Content-Type"];
		//					Respond(result, bytes, contentType: contentType);
		//				}
		//			}
		//			catch (FileNotFoundException)
		//			{
		//				RespondNotFound(result);
		//			}
		//		}
		//	}
		//	else if (requestPath.StartsWith("/songs/"))
		//	{
		//		string query = requestPath.Substring("/songs/".Length);
		//		if (query == "list")
		//		{
		//			if (requestMethod != "GET")
		//				RespondMethodNotAllowed(result);

		//			RespondGzip(result, JsonConvert.SerializeObject(songs.All()));
		//		}
		//		else if (query == "count")
		//		{
		//			if (requestMethod != "GET")
		//				RespondMethodNotAllowed(result);

		//			Respond(result, songs.Count().ToString());
		//		}
		//		else if (query.StartsWith("filter/"))
		//		{
		//			if (requestMethod != "GET")
		//				RespondMethodNotAllowed(result);

		//			query = query.Substring("filter/".Length);
		//			var i = query.IndexOf('/');
		//			if (i < 0)
		//				RespondNotFound(result);

		//			var filter = query.Substring(0, i);
		//			var filterQuery = SongData.NormalizeSearchString(query.Substring(i + 1));

		//			if (filter == "text")
		//			{
		//				RespondGzip(result, JsonConvert.SerializeObject(songs.WhereTextContains(filterQuery)));
		//			}
		//			else if (filter == "title")
		//			{
		//				RespondGzip(result, JsonConvert.SerializeObject(songs.WhereTitleContains(filterQuery)));
		//			}
		//			else if (filter == "source")
		//			{
		//				RespondGzip(result, JsonConvert.SerializeObject(songs.WhereSourceContains(filterQuery)));
		//			}
		//			else if (filter == "copyright")
		//			{
		//				RespondGzip(result, JsonConvert.SerializeObject(songs.WhereCopyrightContains(filterQuery)));
		//			}
		//			else
		//			{
		//				RespondNotFound(result); // unsupported filter method
		//			}
		//		}
		//		else
		//		{
		//			if (requestMethod == "GET")
		//			{
		//				try
		//				{
		//					using (var stream = songs.Get(query))
		//					{
		//						// TODO: send Last-Modified header
		//						Respond(result, ReadStream(stream), contentType: "text/xml");
		//					}
		//				}
		//				catch (FileNotFoundException)
		//				{
		//					RespondNotFound(result);
		//				}
		//				catch (ArgumentException)
		//				{
		//					RespondNotFound(result);
		//				}
		//			}
		//			else if (requestMethod == "PUT")
		//			{
		//				var contentLength = int.Parse(((IDictionary<string, IEnumerable<string>>)env["owin.RequestHeaders"])["Content-Length"].Single());
		//				var requestBody = (BodyDelegate)env["owin.RequestBody"];

		//				var responseBody = Server.Utils.Extensions.BufferedRequestBody(requestBody, contentLength, (bytes) =>
		//					{
		//						using (var ft = songs.Put(query))
		//						{
		//							ft.Stream.Write(bytes, 0, bytes.Length);
		//						}
		//					});

		//				result(
		//					"200 OK",
		//					new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
		//				{
		//					{"Content-Type", new[] {"text/plain"}},
		//				},
		//					responseBody
		//				);
		//			}
		//			else if (requestMethod == "DELETE")
		//			{
		//				try
		//				{
		//					songs.Delete(query);
		//					Respond(result, "OK");
		//				}
		//				catch (FileNotFoundException)
		//				{
		//					RespondNotFound(result);
		//				}
		//			}
		//		}
		//	}
		//	else
		//	{
		//		Respond(result, HtmlContent.Replace("###", requestPath), contentType: "text/html");
		//	}
		//}

		//private void Respond(ResultDelegate del, string response, string contentType = "text/plain", string code = "200 OK")
		//{
		//	Respond(del, Encoding.UTF8.GetBytes(response), contentType + "; charset=utf-8", code);
		//}

		//private void RespondGzip(ResultDelegate del, string response, string contentType = "text/plain", string code = "200 OK")
		//{
		//	var inStream = new MemoryStream(Encoding.UTF8.GetBytes(response));
		//	var outStream = new MemoryStream();
		//	using (GZipStream tinyStream = new GZipStream(outStream, CompressionMode.Compress))
		//	{
		//		inStream.CopyTo(tinyStream);
		//	}
		//	Respond(del, outStream.ToArray(), contentType + "; charset=utf-8", code, "gzip");
		//	outStream.Close();
		//	inStream.Close();
		//}

		//private void Respond(ResultDelegate del, byte[] response, string contentType = "text/plain", string code = "200 OK", string contentEncoding = null)
		//{
		//	var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

		//	if (contentType != null)
		//		headers.Add("Content-Type", new[] { contentType });

		//	if (contentEncoding != null)
		//		headers.Add("Content-Encoding", new[] { contentEncoding });

		//	del(
		//		code,
		//		headers,
		//		(write, flush, end, cancel) =>
		//		{
		//			write(new ArraySegment<byte>(response));
		//			end(null);
		//		}
		//	);
		//}

		//private void RespondNotFound(ResultDelegate del)
		//{
		//	Respond(del, "Not Found", code: "404 Not Found");
		//}

		//private void RespondMethodNotAllowed(ResultDelegate del)
		//{
		//	Respond(del, "Request method not allowed.", code: "405 Method Not Allowed");
		//}

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
