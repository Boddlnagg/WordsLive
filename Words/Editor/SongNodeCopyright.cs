
using MonitoredUndo;
namespace Words.Editor
{
	public class SongNodeCopyright : SongNodeMetadata
	{
		public SongNodeCopyright(SongNodeRoot root) : base(root)
		{
			this.Title = Words.Resources.Resource.eMetadataCopyrightTitle;
			this.Text = root.Song.Copyright;
		}

		protected override void UpdateSource(string value)
		{
			Root.Song.Copyright = value;
		}

		public override string IconUri
		{
			get { return "/Words;component/Artwork/Small_Copyright.png"; }
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
				Root.Song.Formatting.CopyrightText.Size = value;
				OnNotifyPropertyChanged("FontSize");
			}
		}
	}
}
