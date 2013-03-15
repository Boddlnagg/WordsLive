using System.Windows.Media;
using WordsLive.Core.Songs;
using WordsLive.MediaOrderList;

namespace WordsLive.Songs
{
	[TargetMedia(typeof(SongMedia))]
	public class SongIconProvider : IconProvider
	{
		public SongIconProvider(SongMedia data) : base(data)
		{ }

		protected override ImageSource CreateIcon()
		{ 
			// TODO: use SongStorage's method to get an icon?

			SongMedia s = (SongMedia)this.Data;
			return SongBackgroundToImageSourceConverter.CreateBackgroundSource(s.Song.Backgrounds[0], 22);
		}
	}
}
