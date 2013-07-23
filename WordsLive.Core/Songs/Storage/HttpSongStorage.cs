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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs.Storage
{
	public class HttpSongStorage : SongStorage
	{
		private WebClient client;

		private HttpClient client2;

		class GZipWebClient : WebClient
		{
			protected override WebRequest GetWebRequest(Uri address)
			{
				HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				return request;
			}
		}

		public HttpSongStorage(string baseAddress, NetworkCredential credential = null)
		{
			this.client = new GZipWebClient();
			client.Encoding = System.Text.Encoding.UTF8;
			this.client2 = new HttpClient(new HttpClientHandler { Credentials = credential });
			client2.BaseAddress = new Uri(baseAddress);
			client.BaseAddress = baseAddress;
			if (credential != null)
				client.Credentials = credential;
		}

		public override IEnumerable<SongData> All()
		{
			return FetchSongData("list");
		}

		public override Task<IEnumerable<SongData>> AllAsync()
		{
			return FetchSongDataAsync("list");
		}

		public override IEnumerable<SongData> WhereTitleContains(string query)
		{
			return FetchSongData("filter/title/" + query);
		}

		public override IEnumerable<SongData> WhereTextContains(string query)
		{
			return FetchSongData("filter/text/" + query);
		}

		public override IEnumerable<SongData> WhereCopyrightContains(string query)
		{
			return FetchSongData("filter/copyright/" + query);
		}

		public override IEnumerable<SongData> WhereSourceContains(string query)
		{
			return FetchSongData("filter/source/" + query);
		}

		public override int Count()
		{
			var result = client2.GetStringAsync("count").WaitAndUnwrapException();
			return int.Parse(result);
		}

		public override Stream Get(string name)
		{
			return GetAsync(name).WaitAndUnwrapException();
		}

		public override async Task<Stream> GetAsync(string name, CancellationToken cancellation = default(CancellationToken))
		{
			var result = await client2.GetAsync(name, cancellation).ConfigureAwait(false);
			if (result.StatusCode == HttpStatusCode.NotFound)
				throw new FileNotFoundException();
			else if (!result.IsSuccessStatusCode)
				throw new HttpRequestException();

			return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
		}

		public override FileTransaction Put(string path)
		{
			return new HttpFileTransaction(path, client);
		}

		public override void Delete(string name)
		{
			var result = client.UploadString(name, "DELETE", "");
			if (result != "OK")
				throw new WebException("Deleting failed.");
		}

		public override FileInfo GetLocal(string name)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.Combine(DataManager.TempDirectory.FullName, "SongsLocal"));
			if (!dir.Exists)
				dir.Create();

			FileInfo fi = new FileInfo(Path.Combine(dir.FullName, name));

			if (fi.Exists)
				return fi;

			using (var stream = Get(name))
			{
				using (FileStream fs = File.OpenWrite(fi.FullName))
				{
					stream.CopyTo(fs);
				}
			}

			return fi;
		}

		public override bool Exists(string name)
		{
			try
			{
				var stream = Get(name);
				stream.Close();
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		private IEnumerable<SongData> FetchSongData(string path)
		{
			var result = client2.GetStringAsync(path).WaitAndUnwrapException();
			return JsonConvert.DeserializeObject<IEnumerable<SongData>>(result);
		}

		private async Task<IEnumerable<SongData>> FetchSongDataAsync(string path)
		{
			var result = await client2.GetStringAsync(path).ConfigureAwait(false);
			return JsonConvert.DeserializeObject<IEnumerable<SongData>>(result);
		}
	}
}
