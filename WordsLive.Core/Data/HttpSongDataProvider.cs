﻿/*
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
using Newtonsoft.Json;

namespace WordsLive.Core.Data
{
	public class HttpSongDataProvider : SongDataProvider, IBidirectionalMediaDataProvider
	{
		private WebClient client;

		public HttpSongDataProvider(string baseAddress, NetworkCredential credential = null)
		{
			this.client = new WebClient();
			client.BaseAddress = baseAddress;
			if (credential != null)
				client.Credentials = credential;
		}

		public override IEnumerable<SongData> All()
		{
			return FetchSongData("list");
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
			var result = client.DownloadString("count");
			return int.Parse(result);
		}

		public override Stream Get(string path)
		{
			try
			{
				var result = client.DownloadData(path);
				return new MemoryStream(result);
			}
			catch (WebException e)
			{
				if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound) // 404
					throw new FileNotFoundException();
				else
					throw;
			}
		}

		public override Uri GetUri(string path)
		{
			throw new NotSupportedException();
		}

		public override FileInfo GetLocal(string path)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.Combine(DataManager.TempDirectory.FullName, "SongsLocal"));
			if (!dir.Exists)
				dir.Create();

			FileInfo fi = new FileInfo(Path.Combine(dir.FullName, path));

			if (fi.Exists)
				return fi;

			using (var stream = Get(path))
			{
				using (FileStream fs = File.OpenWrite(fi.FullName))
				{
					stream.CopyTo(fs);
				}
			}

			return fi;
		}

		public FileTransaction Put(string path)
		{
			return new HttpFileTransaction(path, client);
		}

		public void Delete(string path)
		{
			var result = client.UploadString(path, "DELETE", "");
			if (result != "OK")
				throw new WebException("Deleting failed.");
		}

		private IEnumerable<SongData> FetchSongData(string path)
		{
			var result = client.DownloadString(path);
			return JsonConvert.DeserializeObject<IEnumerable<SongData>>(result);
		}
	}
}
