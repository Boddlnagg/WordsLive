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
using System.Text;
using System.Text.RegularExpressions;
using Owin;

namespace WordsLive.Server.Utils
{
	public static class DigestAuthentication
	{
		public struct UserPassword
		{
			public string Password;
			public bool IsHashed;

			internal string GetHA1(string user, string realm)
			{
				if (IsHashed)
					return Password;
				else
					return (user + ":" + realm + ":" + Password).ComputeMD5Hash();
			}
		}

		public static AppDelegate Enable(AppDelegate app, Func<IDictionary<string, object>, bool> requiresAuth, string realm, Func<string, UserPassword?> authenticator)
		{
			return (env, result, fault) =>
			{
				if (!requiresAuth(env))
				{
					app(env, result, fault);
					return;
				}

				var requestHeaders = (IDictionary<string, IEnumerable<string>>)env["owin.RequestHeaders"];

				var header = requestHeaders.GetHeader("Authorization");

				var parsedHeader = ParseDigestHeader(header);

				if (parsedHeader == null)
				{
					Unauthorized(result, realm);
					return;
				}

				string user = parsedHeader["username"];
				var pwd = authenticator(user);

				// TODO: check for increment of "nc" header value

				if (!pwd.HasValue || !IsValidResponse(pwd.Value.GetHA1(user, realm), (string)env["owin.RequestMethod"], parsedHeader))
				{
					Unauthorized(result, realm);
					return;
				}

				env["gate.RemoteUser"] = user;
				app(env, result, fault);
				return;
			};
		}

		private static void Unauthorized(ResultDelegate result, string realm)
		{
			result(
				"401 Unauthorized",
				new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
				{
					{
						"WWW-Authenticate", new []
						{
							"Digest realm=\"" + realm + "\"," +
							"qop=\"auth\"," +
							"nonce=\"" + GenerateNonce() + "\"," +
							"opaque=\"" + realm.ComputeMD5Hash() + "\""
						}
					}
				},
				(write, flush, end, cancel) =>
				{
					var bytes = Encoding.Default.GetBytes("Authorization required");
					write(new ArraySegment<byte>(bytes));
					end(null);
				});
		}

		private static string GenerateNonce()
		{
			return DateTime.Now.ToString("yyyyMMdd") + Guid.NewGuid().ToString().Substring(13);
		}

		private static IDictionary<string, string> ParseDigestHeader(string header)
		{
			if (String.IsNullOrEmpty(header))
				return null;

			if (!header.StartsWith("Digest ", StringComparison.InvariantCultureIgnoreCase))
				return null;

			header = header.Substring("Digest ".Length).TrimStart();

			var data = new Dictionary<string, string>();
			var neededParts = new HashSet<string> { "nonce", "nc", "cnonce", "qop", "username", "uri", "response" };

			var regex = new Regex("(\\w+)[:=] ?\"?([^\" ,]+)\"?");
			foreach (Match match in regex.Matches(header, 0))
			{
				var key = match.Groups[1].Value.Trim();
				string value;
				if (key == "uri")
				{
					// uri value might contain commas
					int start = match.Groups[2].Index;
					int end = header.IndexOf('"', start);
					value = header.Substring(start, end - start);
				}
				else
				{
					value = match.Groups[2].Value.Trim('"', '\'');
				}
				data[key] = value;
				neededParts.Remove(key);
			}

			if (neededParts.Count > 0)
				return null;

			return data;
		}

		private static bool IsValidResponse(string ha1, string requestMethod, IDictionary<string, string> digestHeader)
		{
			var ha2 = (requestMethod + ":" + digestHeader["uri"]).ComputeMD5Hash();
			var validResponse = (ha1 + ":" +
				digestHeader["nonce"] + ":" +
				digestHeader["nc"] + ":" +
				digestHeader["cnonce"] + ":" +
				digestHeader["qop"] + ":" + ha2).ComputeMD5Hash();
			return digestHeader["response"] == validResponse;
		}
	}
}
