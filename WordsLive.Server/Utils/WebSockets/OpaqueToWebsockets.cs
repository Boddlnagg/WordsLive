using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Owin.Types;
using Owin.Types.Helpers;

namespace WordsLive.Server.Utils.WebSockets
{
	using AppFunc = Func<IDictionary<string, object>, Task>;
	using WebSocketAccept =
					Action
					<
						IDictionary<string, object>, // WebSocket Accept parameters
						Func // WebSocketFunc callback
						<
							IDictionary<string, object>, // WebSocket environment
							Task // Complete
						>
					>;
	using WebSocketFunc =
					Func
					<
						IDictionary<string, object>, // WebSocket environment
						Task // Complete
					>;

	// This class demonstrates how to support WebSockets on a server that only supports opaque streams.
	// WebSocket Extension v0.4 is currently implemented.
	public static class OpaqueToWebSocket
	{
		public static IAppBuilder UseWebSockets(this IAppBuilder builder)
		{
			return builder.UseFunc<AppFunc>(OpaqueToWebSocket.Middleware);
		}

		public static AppFunc Middleware(AppFunc app)
		{
			return env =>
			{
				var request = new OwinRequest(env);
				var websocketVersion = request.Get<string>(OwinConstants.WebSocket.Version);
				WebSocketAccept webSocketAccept = request.Get<WebSocketAccept>(OwinConstants.WebSocket.Accept);

				if (request.CanUpgrade && websocketVersion == null) // If we have opaque support and no WebSocket support yet
				{
					if (IsWebSocketRequest(env))
					{
						// TODO: check for correct requested version of WebSocket protocol
						IDictionary<string, object> acceptOptions = null;
						WebSocketFunc webSocketFunc = null;

						// Announce websocket support
						env[OwinConstants.WebSocket.Accept] = new WebSocketAccept(
							(options, callback) =>
							{
								acceptOptions = options;
								webSocketFunc = callback;
								env[OwinConstants.ResponseStatusCode] = 101;
							});


						return app(env).ContinueWith(t =>
						{
							OwinResponse response = new OwinResponse(env);
							if (response.StatusCode == 101
								&& webSocketFunc != null)
							{
								SetWebSocketResponseHeaders(env, acceptOptions);

								request.UpgradeDelegate(acceptOptions, opaqueEnv =>
								{
									WebSocketLayer webSocket = new WebSocketLayer(opaqueEnv);
									return webSocketFunc(webSocket.Environment)
										.ContinueWith(tt => webSocket.CleanupAsync());
								});
							}
						});
					}
				}

				// else
				return app(env);
			};
		}

		// Inspect the method and headers to see if this is a valid websocket request.
		// See RFC 6455 section 4.2.1.
		private static bool IsWebSocketRequest(IDictionary<string, object> env)
		{
			var requestHeaders = (IDictionary<string, string[]>)env[OwinConstants.RequestHeaders];
			var webSocketUpgrade = OwinHelpers.GetHeaderSplit(requestHeaders, "Upgrade");
			var webSocketVersion = OwinHelpers.GetHeader(requestHeaders, "Sec-WebSocket-Version");
			var webSocketKey = OwinHelpers.GetHeader(requestHeaders, "Sec-WebSocket-Key");
			return webSocketUpgrade != null &&
				webSocketUpgrade.Contains("websocket", StringComparer.OrdinalIgnoreCase) &&
				webSocketVersion != null && webSocketKey != null;
		}

		// Se the websocket response headers.
		// See RFC 6455 section 4.2.2.
		private static void SetWebSocketResponseHeaders(IDictionary<string, object> env, IDictionary<string, object> acceptOptions)
		{
			var requestHeaders = (IDictionary<string, string[]>)env[OwinConstants.RequestHeaders];
			var webSocketKey = OwinHelpers.GetHeader(requestHeaders, "Sec-WebSocket-Key");

			env[OwinConstants.ResponseReasonPhrase] = "Switching Protocols";
			var responseHeaders = (IDictionary<string, string[]>)env[OwinConstants.ResponseHeaders];
			OwinHelpers.AddHeader(responseHeaders, "Connection", "Upgrade");
			OwinHelpers.AddHeader(responseHeaders, "Upgrade", "websocket");
			var webSocketAccept = Convert.ToBase64String(
				SHA1.Create().ComputeHash(Encoding.Default.GetBytes(webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
			OwinHelpers.AddHeader(responseHeaders, "Sec-WebSocket-Accept", webSocketAccept);
		}
	}
}
