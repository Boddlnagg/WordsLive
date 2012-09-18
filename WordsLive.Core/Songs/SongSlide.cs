using System;
using System.Collections.Generic;
using System.Text;

namespace Words.Core.Songs
{
	public class SongSlide
	{
		private string text;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
				TextWithoutChords = Chords.Chords.RemoveAll(text);
			}
		}

		public string TextWithoutChords { get; private set; }
		public string Translation { get; set; }
		public int BackgroundIndex { get; set; }
		public int Size { get; set; }
	}
}
