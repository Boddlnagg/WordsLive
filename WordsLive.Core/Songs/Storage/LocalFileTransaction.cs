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
using System.Threading.Tasks;

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Implements a file transaction using a local file.
	/// </summary>
	public class LocalFileTransaction : FileTransaction
	{
		private FileStream stream;

		public override Stream Stream
		{
			get { return stream; }
		}

		public LocalFileTransaction(string path)
		{
			stream = new FileStream(path, FileMode.Create);
		}

		protected override void DoFinish()
		{
			stream.Close();
		}

		protected override Task DoFinishAsync()
		{
			stream.Close();
			return TaskHelpers.Completed();
		}
	}
}
