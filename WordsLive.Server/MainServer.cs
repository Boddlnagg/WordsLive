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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firefly.Http;
using Owin;
using Owin.Builder;
using Owin.Types;
using WordsLive.Core;
using WordsLive.Core.Songs.Storage;
using WordsLive.Server.Utils;
using WordsLive.Server.Utils.WebSockets;

namespace WordsLive.Server
{
	using AppFunc = Func<IDictionary<string, object>, Task>;

	static class Workaround
	{
		#pragma warning disable 169
		private static Action _1;
		private static Func<int, int, int> _2;
		private static Func<int, int> _3;
		#pragma warning restore 169
	}

	public class MainServer
	{
		/// <summary>
		/// Default start page
		/// </summary>
		private string HtmlContent
		{
			get
			{
				return
@"<!DOCTYPE html>
<html>
<head>
<title>WordsLive Server</title>
</head>
<body>
<h2>Welcome to WordsLive Server</h2>
<p>Everything is up and running.</p>
</body>
</html>";
			}
		}

		/// <summary>
		/// Test page for WebSocket test
		/// </summary>
		private string HtmlContentEcho
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

		public MainServer(int port)
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
				else if (request.Path == "/Echo")
				{
					var response = new OwinResponse(env);
					return response.RespondString(HtmlContentEcho, "text/html");
				}
				else if (request.Path == "/")
				{
					var response = new OwinResponse(env);
					return response.RespondString(HtmlContent, "text/html");
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
			builder.UseType<BackgroundsModule>();
			builder.UseType<SongsModule>();

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
	}
}
