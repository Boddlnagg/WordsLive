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
using System.Text;

namespace WordsLive.Core.Data
{
	/// <summary>
	/// Represents a file transaction that uploads data using an HTTP PUT request.
	/// </summary>
	public class HttpFileTransaction : FileTransaction
	{
		private MemoryStream stream;
		private Uri uri;
		private NetworkCredential credential;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpFileTransaction"/> class.
		/// </summary>
		/// <param name="uri">The URI to upload to.</param>
		/// <param name="credential">The credentials.</param>
		public HttpFileTransaction(Uri uri, NetworkCredential credential = null)
		{
			stream = new MemoryStream();
			this.uri = uri;
			this.credential = credential;
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
			using (var client = new WebClient())
			{
				if (credential != null)
					client.Credentials = credential;

				var result = Encoding.ASCII.GetString(client.UploadData(uri, "PUT", stream.ToArray()));
				if (result != "OK")
					throw new WebException("Uploading failed");
			}
		}
	}
}
