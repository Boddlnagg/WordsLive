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
using System.IO;
using System.Net;

namespace WordsLive.Core.Data
{
	public class HttpDataProvider : IMediaDataProvider
	{
		private WebClient client;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpDataProvider"/> class.
		/// </summary>
		public HttpDataProvider()
		{
			this.client = new WebClient();
		}

		public Stream Get(string path)
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

		public Uri GetUri(string path)
		{
			return new Uri(path);
		}

		public FileInfo GetLocal(string path)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.Combine(DataManager.TempDirectory.FullName, "DownloadLocal"));
			if (!dir.Exists)
				dir.Create();

			// create a random subdirectory, so the original filename can be used
			string subdir = Path.GetRandomFileName();
			string p;
			while (Directory.Exists(p = Path.Combine(dir.FullName, subdir)))
			{
				subdir = Path.GetRandomFileName();
			}

			Directory.CreateDirectory(p);

			string localPath = Path.Combine(p, Path.GetFileName(path));

			FileInfo fi = new FileInfo(localPath);

			using (var stream = Get(path))
			{
				using (FileStream fs = File.OpenWrite(fi.FullName))
				{
					stream.CopyTo(fs);
				}
			}

			return fi;
		}
	}
}
