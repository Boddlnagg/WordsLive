
using MonitoredUndo;
namespace WordsLive.Editor
{
	public class SongNodeCopyright : SongNodeMetadata
	{
		public SongNodeCopyright(SongNodeRoot root) : base(root)
		{
			this.Title = WordsLive.Resources.Resource.eMetadataCopyrightTitle;
			this.Text = root.Song.Copyright;
		}

		protected override void UpdateSource(string value)
		{
			Root.Song.Copyright = value;
		}

		public override string IconUri
		{
			get { return "/WordsLive;component/Artwork/Small_Copyright.png"; }
		}

		public int FontSize
		{
			get
			{
				return Root.Song.Formatting.CopyrightText.Size;
			}
			set
			{
				DefaultChangeFactory.OnChanging(this, "FontSize", Root.Song.Formatting.CopyrightText.Size, value);
				var formatting = Root.Song.Formatting;
				formatting.CopyrightText.Size = value;
				Root.Song.Formatting = formatting;
				OnNotifyPropertyChanged("FontSize");
			}
		}
	}
}
