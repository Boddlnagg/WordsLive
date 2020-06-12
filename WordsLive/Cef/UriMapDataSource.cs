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

using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace WordsLive.Cef
{
	public class UriMapDataSource
	{
		private static UriMapDataSource instance;

		public static UriMapDataSource Instance
		{
			get
			{
				if (instance == null)
					instance = new UriMapDataSource();

				return instance;
			}
		}

		private object o = new object();

		private Dictionary<string, Uri> map = new Dictionary<string, Uri>();

		public void Add(string key, Uri uri)
		{
			if (uri.IsFile && !File.Exists(uri.LocalPath))
				throw new FileNotFoundException(uri.LocalPath);

			lock (o)
			{
				map.Add(key, uri);
			}
		}

		public void Remove(string key)
		{
			lock (o)
			{
				map.Remove(key);
			}
		}

		public bool Exists(string key)
		{
			lock (o)
			{
				return map.ContainsKey(key);
			}
		}

		public IResourceHandler CreateHandler(Uri url)
		{
			try
			{
				string path = url.AbsolutePath.Substring(1); // remove leading slash
				Uri localUrl;
				lock (o)
				{
					localUrl = map[path];
				}

				if (!localUrl.IsFile)
					throw new NotImplementedException("Mapping to non-local URIs not yet implemented.");

				string mimeType = CefSharp.Cef.GetMimeType(Path.GetExtension(localUrl.LocalPath));
				return ResourceHandler.FromFilePath(localUrl.LocalPath, mimeType);
			}
			catch
			{
				return null;
			}
		}
	}
}
