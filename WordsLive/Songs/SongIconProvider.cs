using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordsLive.Core.Data;
using WordsLive.Core.Songs;
using WordsLive.MediaOrderList;

namespace WordsLive.Songs
{
	[TargetMedia(typeof(Song))]
	public class SongIconProvider : IconProvider
	{
		public SongIconProvider(Song data) : base(data)
		{ }

		protected override ImageSource CreateIcon()
		{ 
			// TODO: use provider's method to get an icon?

			Song s = (Song)this.Data; // this only contains title and backgrounds
			return SongBackgroundToImageSourceConverter.CreateBackgroundSource(s.Backgrounds[0], 22);
		}
	}
}
