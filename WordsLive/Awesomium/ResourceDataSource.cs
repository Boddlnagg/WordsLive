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
using System.Reflection;
using System.Runtime.InteropServices;
using Awesomium.Core.Data;
using WordsLive.Core;

namespace WordsLive.Awesomium
{
	public class ResourceDataSource : IDataSource
	{
		private Assembly assembly;
		public ResourceDataSource(Assembly assembly)
		{
			this.assembly = assembly;
		}

		public bool HandleRequest(DataSourceRequest request, Action<DataSourceResponse> respond)
		{
			try
			{
				using (var stream = assembly.GetResourceStream(request.Path))
				{
					byte[] buffer = new byte[stream.Length];
					stream.Read(buffer, 0, (int)stream.Length);
					GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					IntPtr pointer = pinnedBuffer.AddrOfPinnedObject();
					respond(new DataSourceResponse
					{
						Buffer = pointer,
						Size = (uint)stream.Length
					});
					pinnedBuffer.Free();
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
