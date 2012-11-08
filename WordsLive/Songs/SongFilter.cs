using System.Linq;
using WordsLive.Core.Data;
using WordsLive.Utils;

namespace WordsLive.Songs
{
	class SongFilter
	{
		private string keyword;

		public string Keyword
		{
			get
			{
				return keyword;
			}
			set
			{
				keyword = value;
				NormalizedKeyword = SongData.NormalizeSearchString(keyword);
			}
		}

		public string NormalizedKeyword { get; private set; }

		public bool SearchInText
		{
			get
			{
				return Properties.Settings.Default.SongListSearchInText;
			}
			set
			{
				Properties.Settings.Default.SongListSearchInText = value;
			}
		}
		public string Source { get; set; }

		public string Copyright { get; set; }

		public SongFilter()
		{
			Reset();
		}

		public bool Matches(SongData song)
		{
			if (IsEmpty)
			{
				return true;
			}

			if (Source != "")
			{
				if (!song.Sources.ContainsIgnoreCase(Source))
				{
					return false;
				}
			}

			if (Copyright != "")
			{
				if (!song.Copyright.ContainsIgnoreCase(Copyright))
				{
					return false;
				}
			}

			if (Keyword != "")
			{
				if (!(song.SearchTitle.ContainsIgnoreCase(NormalizedKeyword) || (SearchInText && song.SearchText.ContainsIgnoreCase(NormalizedKeyword))))
				{
					return false;
				}
			}

			return true;
		}

		public bool IsEmpty
		{
			get
			{
				return Keyword == "" && Source == "" && Copyright == "";
			}
		}

		public void Reset()
		{
			Keyword = "";
			Source = "";
			Copyright = "";
		}
	}
}
