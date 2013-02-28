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

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Represents a file transaction initiated by the <see cref="IMediaDataProvider.Put"/> method.
	/// </summary>
	public abstract class FileTransaction : IDisposable
	{
		/// <summary>
		/// Gets a value indicating whether the transaction has already been finished.
		/// </summary>
		public bool IsFinished { get; private set; }

		/// <summary>
		/// Gets the stream to write to. Don't close this stream!
		/// </summary>
		public abstract Stream Stream { get; }

		/// <summary>
		/// Finishes the transaction.
		/// </summary>
		public void Finish()
		{
			if (IsFinished)
				throw new InvalidOperationException("Transaction already finished.");

			DoFinish();
		}

		/// <summary>
		/// Actual implementation called by the Finish method.
		/// </summary>
		protected abstract void DoFinish();

		public void Dispose()
		{
			if (!IsFinished)
				Finish();
		}
	}
}
