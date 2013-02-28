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

using System.IO;
using System.Net;
using System.Text;

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Represents a file transaction that uploads data using an HTTP PUT request.
	/// </summary>
	public class HttpFileTransaction : FileTransaction
	{
		private MemoryStream stream;
		private string relativeUri;
		private WebClient client;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpFileTransaction"/> class.
		/// </summary>
		/// <param name="relativeUri">The URI to upload to (relative to the base address set in the WebClient).</param>
		/// <param name="client">The WebClient to use.</param>
		public HttpFileTransaction(string relativeUri, WebClient client)
		{
			stream = new MemoryStream();
			this.relativeUri = relativeUri;
			this.client = client;
		}
		
		public override Stream Stream
		{
			get
			{
				return stream;
			}
		}

		protected override void DoFinish()
		{
			stream.Close();
			var result = Encoding.ASCII.GetString(client.UploadData(relativeUri, "PUT", stream.ToArray()));
			if (result != "OK")
				throw new WebException("Uploading failed");
		}
	}
}
