using System;
using System.IO;

namespace WordsLive.Core.Songs.IO
{
	public interface ISongWriter
	{
		/// <summary>
		/// Writes the song data to a stream.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <param name="stream">The stream.</param>
		void Write(Song song, Stream stream);
	}
}
