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
using System.Linq;
using System.Net;

namespace WordsLive.Core.Songs.Storage
{
	public class HttpBackgroundStorage : BackgroundStorage
	{
		private WebClient client;
		private string baseAddress;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpBackgroundStorage"/> class.
		/// </summary>
		/// <param name="baseAddress">The root address, e.g. http://example.com/backgrounds/). </param>
		/// <param name="credentials">The credentials, if needed.</param>
		public HttpBackgroundStorage(string baseAddress, NetworkCredential credential = null)
		{
			this.client = new WebClient();
			this.baseAddress = baseAddress;
			client.BaseAddress = baseAddress;
			if (credential != null)
				client.Credentials = credential;
		}

		public override BackgroundFile GetFile(string path)
		{
			int i = path.LastIndexOf('/');
			var directory = new BackgroundDirectory(this, path.Substring(0, i + 1));
			var entries = GetListing(directory).Where(e => e.Path == path);
			if (!entries.Any())
				throw new FileNotFoundException(path);

			return new BackgroundFile(this, directory, path.Substring(i + 1), entries.Single().IsVideo);
		}

		public override IEnumerable<BackgroundFile> GetFiles(BackgroundDirectory directory)
		{
			return GetListing(directory).Where(e => !e.IsDirectory).Select(e => new BackgroundFile(this, directory, Path.GetFileName(e.Path), e.IsVideo)).OrderBy(f => f.Name);
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
			try
			{
				var result = client.DownloadString(directory.Path.Substring(1) + "list");
				return result.Split('\n').Where(p => p.Trim() != String.Empty).Select(p => new ListingEntry(p)).ToArray();
			}
			catch (WebException)
			{
				throw new FileNotFoundException();
			}
		}

		/// <summary>
		/// Helper class for single listing entries, used by the GetListing method
		/// </summary>
		private class ListingEntry
		{
			public string Path { get; private set; }
			public bool IsVideo { get; private set; }
			public bool IsDirectory { get; private set; }

			public ListingEntry(string entry)
			{
				entry = entry.Trim();
				IsDirectory = entry.EndsWith("/");

				if (!IsDirectory && entry.EndsWith(" [VIDEO]", StringComparison.InvariantCultureIgnoreCase))
				{
					Path = entry.Substring(0, entry.Length - 8).TrimEnd();
					IsVideo = true;
				}
				else
				{
					Path = entry;
					IsVideo = false;
				}
			}
		}
	}
}
