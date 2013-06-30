using System;

namespace WordsLive.Core.Songs.IO
{
	public class SongFormatException : Exception
	{
		public SongFormatException(string message) : base(message) { }

		public SongFormatException(string message, Exception innerException) : base(message, innerException) { }
	}
}
