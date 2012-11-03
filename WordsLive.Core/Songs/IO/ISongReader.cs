using System;
using System.IO;

namespace WordsLive.Core.Songs.IO
{
	public interface ISongReader
	{
		/// <summary>
		/// Reads the song data from a stream.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <param name="stream">The stream.</param>
		void Read(Song song, Stream stream);
	}
}
