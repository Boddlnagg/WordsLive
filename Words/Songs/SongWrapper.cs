using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core.Songs;
using System.Windows.Media.Imaging;

namespace Words.Songs
{
	public class SongWrapper
	{
		public SongWrapper(Song song, string path)
		{
			this.Song = song;
			this.Path = path;
		}

		public Song Song { get; private set; }
		public string Path { get; private set; }

		public string Title
		{
			get
			{
				return Song.SongTitle;
			}
		}

		public string CopyrightTrimmed
		{
			get
			{
				return string.Join(" ", (from line in Song.Copyright.Split('\n') select line.Trim()).ToArray());
			}
		}

		public string SourcesString
		{
			get
			{
				return string.Join("; ", (from s in Song.Sources select s.ToString()).ToArray());
			}
		}

		public string Language
		{
			get
			{
				return Song.Language;
			}
		}
	}
}
