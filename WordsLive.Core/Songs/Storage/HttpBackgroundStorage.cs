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
using System.Linq;
using System.Net;
using System.Net.Http;

namespace WordsLive.Core.Songs.Storage
{
	public class HttpBackgroundStorage : BackgroundStorage
	{
		private HttpClient client;
		private string baseAddress;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpBackgroundStorage"/> class.
		/// </summary>
		/// <param name="baseAddress">The root address, e.g. http://example.com/backgrounds/). </param>
		/// <param name="credentials">The credentials, if needed.</param>
		public HttpBackgroundStorage(string baseAddress, NetworkCredential credential = null)
		{
			this.client = new HttpClient(new HttpClientHandler { Credentials = credential });
			this.baseAddress = baseAddress;
			client.BaseAddress = new Uri(baseAddress);
		}

		public override BackgroundFile GetFile(string path)
		{
			int i = path.LastIndexOf('/');
			var directory = new BackgroundDirectory(this, path.Substring(0, i + 1));

			var name = path.Substring(i + 1);

			return new BackgroundFile(this, directory, name, IsVideo(name));
		}

		public override IEnumerable<BackgroundFile> GetFiles(BackgroundDirectory directory)
		{
			return GetListing(directory).Where(e => !e.IsDirectory).Select(e => new BackgroundFile(this, directory, Path.GetFileName(e.Path), IsVideo(e.Path))).OrderBy(f => f.Name);
		}

		public override IEnumerable<BackgroundDirectory> GetDirectories(BackgroundDirectory parent)
		{
			return GetListing(parent).Where(e => e.IsDirectory).Select(e => new BackgroundDirectory(this, e.Path)).OrderBy(d => d.Name);
		}

		public override Uri GetFileUri(BackgroundFile file)
		{
			// this requires no authentication
			return new Uri(baseAddress + file.Path.Substring(1));
		}

		public override Uri GetPreviewUri(BackgroundFile file)
		{
			// this requires no authentication
			return new Uri(baseAddress + file.Path.Substring(1) + "/preview");
		}

		/// <summary>
		/// Gets the listing of files and folders using HTTP.
		/// An requested URL looks like http://host/backgrounds/directory/list.
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		private IEnumerable<ListingEntry> GetListing(BackgroundDirectory directory)
		{
			var result = client.GetAsync(directory.Path.Substring(1) + "list").WaitAndUnwrapException();
			if (result.StatusCode == HttpStatusCode.NotFound)
				throw new FileNotFoundException();

			if (!result.IsSuccessStatusCode)
				throw new HttpRequestException();

			var str = result.Content.ReadAsStringAsync().WaitAndUnwrapException();

			return str.Split('\n').Where(p => p.Trim() != String.Empty).Select(p => new ListingEntry(p)).ToArray();
		}

		private bool IsVideo(string path)
		{
			var di = path.LastIndexOf('.');

			if (di == -1)
				return false;

			var ext = path.Substring(di).ToLowerInvariant();

			return AllowedVideoTypes.ContainsKey(ext);
		}

		/// <summary>
		/// Helper class for single listing entries, used by the GetListing method
		/// </summary>
		private class ListingEntry
		{
			public string Path { get; private set; }
			public bool IsDirectory { get; private set; }

			public ListingEntry(string entry)
			{
				Path = entry.Trim();
				IsDirectory = entry.EndsWith("/");
			}
		}

		public override bool FileExists(BackgroundFile file)
		{
			var path = file.Path;
			try
			{
				var entries = GetListing(file.Parent);
				return entries.Any(e => e.Path == path);
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}
	}
}
