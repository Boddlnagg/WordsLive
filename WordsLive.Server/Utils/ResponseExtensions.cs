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
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Owin.Types;

namespace WordsLive.Server.Utils
{
	public static class ResponseExtensions
	{
		public static async Task RespondDownloaded(this OwinResponse response, Uri uri)
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
					var body = await client.GetStreamAsync(uri).ConfigureAwait(false);
					await body.CopyToAsync(response.Body).ConfigureAwait(false);
				}
			}
		}

		public static Task RespondString(this OwinResponse response, string content, string contentType = "text/plain")
		{
			response.SetHeader("Content-Type", contentType + ";charset=utf-8");
			return response.WriteAsync(content);
		}

		public static async Task RespondCompressedString(this OwinResponse response, string content, string contentType = "text/plain")
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

		public static Task RespondNotFound(this OwinResponse response)
		{
			response.StatusCode = 404;
			response.ReasonPhrase = "Not Found";
			return response.WriteAsync("Not Found");
		}

		public static Task RespondMethodNotAllowed(this OwinResponse response)
		{
			response.StatusCode = 405;
			response.ReasonPhrase = "Method Not Allowed";
			return response.WriteAsync("Request method not allowed.");
		}
	}
}
