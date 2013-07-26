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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs.Storage
{
	public class HttpSongStorage : SongStorage
	{
		private HttpClient client;

		public HttpSongStorage(string baseAddress, NetworkCredential credential = null)
		{
			this.client = new HttpClient(new HttpClientHandler {
				Credentials = credential,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			});

			client.BaseAddress = new Uri(baseAddress);
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
			var result = client.GetStringAsync("count").WaitAndUnwrapException();
			return int.Parse(result);
		}

		public override SongStorageEntry Get(string name)
		{
			return GetAsync(name).WaitAndUnwrapException();
		}

		public override async Task<SongStorageEntry> GetAsync(string name, CancellationToken cancellation = default(CancellationToken))
		{
			var result = await client.GetAsync(name, cancellation).ConfigureAwait(false);
			if (result.StatusCode == HttpStatusCode.NotFound)
				throw new FileNotFoundException();
			else if (!result.IsSuccessStatusCode)
				throw new HttpRequestException();

			var stream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);

			return new SongStorageEntry(stream, result.Content.Headers.LastModified);
		}

		public override FileTransaction Put(string path)
		{
			return new HttpFileTransaction(path, client);
		}

		public override void Delete(string name)
		{
			var result = client.DeleteAsync(name).WaitAndUnwrapException();
			if (result.StatusCode == HttpStatusCode.NotFound)
			{
				return;
			}
			else if (!result.IsSuccessStatusCode)
			{
				throw new HttpRequestException("Deleting failed.");
			}
		}

		public override FileInfo GetLocal(string name)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.Combine(DataManager.TempDirectory.FullName, "SongsLocal"));
			if (!dir.Exists)
				dir.Create();

			FileInfo fi = new FileInfo(Path.Combine(dir.FullName, name));

			if (fi.Exists)
				return fi;

			using (var entry = Get(name))
			{
				using (FileStream fs = File.OpenWrite(fi.FullName))
				{
					entry.Stream.CopyTo(fs);
				}
			}

			return fi;
		}

		public override bool Exists(string name)
		{
			try
			{
				var stream = Get(name).Stream;
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
			var result = client.GetStringAsync(path).WaitAndUnwrapException();
			return JsonConvert.DeserializeObject<IEnumerable<SongData>>(result);
		}

		private async Task<IEnumerable<SongData>> FetchSongDataAsync(string path)
		{
			var result = await client.GetStringAsync(path).ConfigureAwait(false);
			return JsonConvert.DeserializeObject<IEnumerable<SongData>>(result);
		}
	}
}
