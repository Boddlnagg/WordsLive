using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core.Songs;
using Words.Utils;

namespace Words.Songs
{
    class SongFilter
    {
        public string Keyword { get; set; }
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
        public string SourceSongbook { get; set; }
        public string SourceNumber { get; set; }
        public string Copyright { get; set; }

        public SongFilter()
        {
            Reset();
            //SearchInText = true;
        }

        public bool Matches(Song song)
        {
            if (IsEmpty)
            {
                return true;
            }

            if (SourceSongbook != "")
            {
                if ((from s in song.Sources where s.Songbook != null && s.Songbook.ContainsIgnoreCase(SourceSongbook) select s).Count() == 0)
                {
                    return false;
                }
            }

            int n;

            if (SourceNumber != "" && int.TryParse(SourceNumber, out n))
            {
                if ((from s in song.Sources where s.Number == n select s).Count() == 0)
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
                if (!(song.SongTitle.ContainsIgnoreCase(Keyword) || (SearchInText && song.TextWithoutChords.ContainsIgnoreCase(Keyword))))
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
                return Keyword == "" && SourceSongbook == "" && SourceNumber == "" && Copyright == "";
            }
        }

        public void Reset()
        {
            Keyword = "";
            SourceSongbook = "";
            SourceNumber = "";
            Copyright = "";
        }
    }
}
