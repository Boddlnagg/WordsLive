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
using System.Runtime.InteropServices;
using Awesomium.Core.Data;

namespace WordsLive.Awesomium
{
	/// <summary>
	/// Data source for Awesomium that maps custom URIs to local or remote URIs.
	/// </summary>
	public class UriMapDataSource : IDataSource
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

		public bool HandleRequest(DataSourceRequest request, Action<DataSourceResponse> respond)
		{
			try
			{
				Uri uri;

				if (!request.Path.StartsWith("urimap/"))
				{
					return false;
				}
				else
				{
					string path = request.Path.Substring("urimap/".Length);

					lock (o)
					{
						uri = map[path];
					}

					if (!uri.IsFile)
						throw new NotImplementedException("Mapping to non-local URIs not yet implemented.");

					using (var stream = File.OpenRead(uri.LocalPath))
					{
						byte[] bytes = new byte[stream.Length];
						stream.Read(bytes, 0, (int)stream.Length);
						GCHandle pinnedBuffer = GCHandle.Alloc(bytes, GCHandleType.Pinned);
						IntPtr pointer = pinnedBuffer.AddrOfPinnedObject();
						respond(new DataSourceResponse
						{
							Buffer = pointer,
							Size = (uint)bytes.Length
						});
						pinnedBuffer.Free();
					}
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
