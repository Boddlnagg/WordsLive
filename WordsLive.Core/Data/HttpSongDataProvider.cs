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
using Newtonsoft.Json;

namespace WordsLive.Core.Data
{
	public class HttpSongDataProvider : SongDataProvider, IBidirectionalMediaDataProvider
	{
		private string rootAddress;
		private NetworkCredential credential;
		private DirectoryInfo tempDirectory;

		private DirectoryInfo TempDirectory
		{
			get
			{
				if (tempDirectory == null)
					tempDirectory = CreateTempDirectory();

				return tempDirectory;
			}
		}

		public HttpSongDataProvider(string rootAddress, NetworkCredential credential = null)
		{
			this.rootAddress = rootAddress;
			this.credential = credential;
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

		public override Stream Get(string path)
		{
			using (var client = new WebClient())
			{
				if (credential != null)
					client.Credentials = credential;

				try
				{
					var result = client.DownloadData(rootAddress + path);
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
		}

		public override Uri GetUri(string path)
		{
			throw new NotSupportedException();
		}

		public override FileInfo GetLocal(string path)
		{
			FileInfo fi = new FileInfo(Path.Combine(TempDirectory.FullName, path));

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

		public FileTransaction Put(string path, bool allowOverwrite)
		{
			return new HttpFileTransaction(new Uri(rootAddress + path), credential);
		}

		public void Delete(string path)
		{
			using (var client = new WebClient())
			{
				if (credential != null)
					client.Credentials = credential;

				var result = client.UploadString(rootAddress + path, "DELETE", "");
				if (result != "OK")
					throw new WebException("Deleting failed.");
			}
		}

		private IEnumerable<SongData> FetchSongData(string path)
		{
			using (var client = new WebClient())
			{
				if (credential != null)
					client.Credentials = credential;

				var result = client.DownloadString(rootAddress + path);
				return JsonConvert.DeserializeObject<IEnumerable<SongData>>(result);
			}
		}

		private DirectoryInfo CreateTempDirectory()
		{
			DirectoryInfo tmpdir;
			do
			{
				tmpdir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
			} while (tmpdir.Exists);

			tmpdir.Create();
			return tmpdir;
		}

		~HttpSongDataProvider()
		{
			try
			{
				// clean up temp directory
				if (tempDirectory != null)
				{
					foreach (var file in tempDirectory.GetFiles())
					{
						file.Delete();
					}

					tempDirectory.Delete();
				}
			}
			catch { }
		}
	}
}
