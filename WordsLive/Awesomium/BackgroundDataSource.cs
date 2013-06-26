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
using System.Net;
using System.Runtime.InteropServices;
using Awesomium.Core.Data;
using WordsLive.Core;

namespace WordsLive.Awesomium
{
	public class BackgroundDataSource : DataSource
	{
		protected override void OnRequest(DataSourceRequest request)
		{
			try
			{
				var bg = DataManager.Backgrounds.GetFile("/" + request.Path);
				using (WebClient client = new WebClient())
				{
					var bytes = client.DownloadData(bg.Uri);
					GCHandle pinnedBuffer = GCHandle.Alloc(bytes, GCHandleType.Pinned);
					IntPtr pointer = pinnedBuffer.AddrOfPinnedObject();
					SendResponse(request, new DataSourceResponse
					{
						Buffer = pointer,
						Size = (uint)bytes.Length
					});
					pinnedBuffer.Free();
				}
			}
			catch
			{
				SendRequestFailed(request);
			}
		}
	}
}
