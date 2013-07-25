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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Owin.Types;
using Owin.Types.Helpers;

namespace WordsLive.Server.Utils
{
	using AppFunc = Func<IDictionary<string, object>, Task>;

	/// <summary>
	/// This is a functional (but probably buggy) implementation of the Digest Auth protocol.
	/// This should be removed once there is an official OWIN middleware for Digest.
	/// </summary>
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

		public static AppFunc Enable(AppFunc next, Func<IDictionary<string, object>, bool> requiresAuth, string realm, Func<string, UserPassword?> authenticator)
		{
			return env =>
			{
				if (!requiresAuth(env))
				{
					return next(env);
				}

				var requestHeaders = (IDictionary<string, string[]>)env[OwinConstants.RequestHeaders];

				var header = OwinHelpers.GetHeader(requestHeaders, "Authorization");

				var parsedHeader = ParseDigestHeader(header);

				if (parsedHeader == null)
				{
					return Unauthorized(env, realm);
				}

				string user = parsedHeader["username"];
				var pwd = authenticator(user);

				// TODO: check for increment of "nc" header value

				if (!pwd.HasValue || !IsValidResponse(pwd.Value.GetHA1(user, realm), (string)env[OwinConstants.RequestMethod], parsedHeader))
				{
					return Unauthorized(env, realm);
				}

				env["gate.RemoteUser"] = user;
				return next(env);
			};
		}

		private static async Task Unauthorized(IDictionary<string, object> env, string realm)
		{
			env[OwinConstants.ResponseStatusCode] = 401;
			env[OwinConstants.ResponseReasonPhrase] = "Unauthorized";

			var responseHeaders = (IDictionary<string, string[]>)env[OwinConstants.ResponseHeaders];

			responseHeaders.Add(
				"WWW-Authenticate",
				new[]
				{
					"Digest realm=\"" + realm + "\"," +
					"qop=\"auth\"," +
					"nonce=\"" + GenerateNonce() + "\"," +
					"opaque=\"" + realm.ComputeMD5Hash() + "\""
				});

			var response = (Stream)env[OwinConstants.ResponseBody];
			var bytes = Encoding.Default.GetBytes("Authorization required");
			await response.WriteAsync(bytes, 0, bytes.Length);
			return;
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

		public static string ComputeMD5Hash(this string str)
		{
			var md5Hasher = MD5.Create();
			return BitConverter.ToString(md5Hasher.ComputeHash(Encoding.Default.GetBytes(str))).Replace("-", "").ToLower();
		}
	}
}
