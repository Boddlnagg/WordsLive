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
using System.Text;
using Firefly.Http;
using WordsLive.Server.Utils;
using Owin;

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
		static readonly string backgroundDir = @"C:\Users\Patrick\Documents\Powerpraise-Dateien\Backgrounds";

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

		private AppDelegate app;
		private int port;
		private IDisposable server;

		public TestServer(int port)
		{
			this.app = App;
			this.port = port;
		}

		public void EnableAuthentication(string username, string password)
		{
			if (server != null)
				throw new InvalidOperationException("Server already running.");

			this.app = DigestAuthentication.Enable(
				this.app,
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
				(user) => (user == username ?
					new DigestAuthentication.UserPassword { Password = password } :
					(DigestAuthentication.UserPassword?)null)
			);
		}

		public void Start()
		{
			var fac = new ServerFactory();
			server = fac.Create(WebSockets.Enable(this.app, "/Echo", OnWebSocketConnection), this.port);
		}

		public void Stop()
		{
			if (server == null)
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
			string requestPath = (string)env["owin.RequestPath"];

			if (requestPath.StartsWith("/backgrounds/"))
			{
				var query = requestPath.Substring("/backgrounds/".Length);
				if (query == "list")
				{
					StringBuilder sb = new StringBuilder();
					var dir = new DirectoryInfo(backgroundDir);
					foreach (var subdir in dir.GetDirectories())
					{
						sb.Append(subdir.Name);
						sb.Append('\n');
					}
					foreach (var subdir in dir.GetFiles())
					{
						sb.Append(subdir.Name);
						sb.Append('\n');
					}

					result(
					"200 OK",
					new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
				{
					{"Content-Type", new[] {"text/plain"}}
				},
					(write, flush, end, cancel) =>
					{
						var bytes = Encoding.Default.GetBytes(sb.ToString());
						write(new ArraySegment<byte>(bytes));
						end(null);
					});
				}
				else
				{
					result("404 Not Found",
					new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
				{
					{"Content-Type", new[] {"text/plain"}}
				},
					(write, flush, end, cancel) =>
					{
						var bytes = Encoding.Default.GetBytes("Not found.");
						write(new ArraySegment<byte>(bytes));
						end(null);
					});
				}
			}
			else
			{
				result(
					"200 OK",
					new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
				{
					{"Content-Type", new[] {"text/html"}}
				},
					(write, flush, end, cancel) =>
					{
						var bytes = Encoding.Default.GetBytes(HtmlContent.Replace("###", requestPath));
						write(new ArraySegment<byte>(bytes));
						end(null);
					});
			}
		}
	}
}
