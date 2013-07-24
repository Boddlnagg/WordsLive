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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Owin;

namespace WordsLive.Server.Utils
{
	public static class Extensions
	{
		public static ArraySegment<byte> Concat(this ArraySegment<byte> target, ArraySegment<byte> source)
		{
			if (target.Array.Length < target.Count + source.Count)
			{
				var grown = new byte[target.Count + source.Count];
				Array.Copy(target.Array, target.Offset, grown, 0, target.Count);
				Array.Copy(source.Array, source.Offset, grown, target.Count, source.Count);
				return new ArraySegment<byte>(grown, 0, target.Count + source.Count);
			}

			if (target.Array.Length - target.Offset < target.Count + source.Count)
			{
				Array.Copy(target.Array, target.Offset, target.Array, 0, target.Count);
				Array.Copy(source.Array, source.Offset, target.Array, target.Count, source.Count);
				return new ArraySegment<byte>(target.Array, 0, target.Count + source.Count);
			}

			Array.Copy(source.Array, source.Offset, target.Array, target.Offset + target.Count, source.Count);
			return new ArraySegment<byte>(target.Array, target.Offset, target.Count + source.Count);
		}

		//public static BodyDelegate BufferedRequestBody(
		//	BodyDelegate requestBody, int contentLength, Action<byte[]> doneAction)
		//{
		//	return
		//		(write, flush, end, cancel) =>
		//		{
		//			var buffer = new ArraySegment<byte>(new byte[contentLength], 0, 0);

		//			requestBody.Invoke(
		//				data =>
		//				{
		//					buffer = buffer.Concat(data);

		//					if (buffer.Count < contentLength)
		//					{
		//						return false;
		//					}
		//					else
		//					{
		//						doneAction(buffer.Array);
		//						write(new ArraySegment<byte>(Encoding.Default.GetBytes("OK")));
		//						return true;
		//					}
		//				},
		//				_ => false,
		//				ex =>
		//				{
		//					end(ex);
		//				},
		//				cancel);
		//		};
		//}

		//public static Action<Action<string>, Action<Exception>> BufferRequestBody(BodyDelegate requestBody, byte[] buffer, StringBuilder sb, int totalBytesRead, int contentLength)
		//{
		//    return (r, e) =>
		//        requestBody(buffer, 0, buffer.Length, bytesRead =>
		//        {
		//            if (bytesRead > 0)
		//            {
		//                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
		//                totalBytesRead += bytesRead;
		//            }

		//            if (totalBytesRead == contentLength)
		//                r(sb.ToString());
		//            else
		//            {
		//                BufferRequestBody(requestBody, buffer, sb, totalBytesRead, contentLength)(r, e);
		//            }
		//        },
		//        exception =>
		//        {
		//            e(new Exception("Error reading request body."));
		//        });
		//}
	}
}
