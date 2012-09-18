using WordsLive.Core.Songs;
using MonitoredUndo;

namespace WordsLive.Editor
{
	public class SongNodeSource : SongNode
	{
		private SongSource source;

		public SongSource Source
		{
			get
			{
				return source;
			}
		}

		public override string IconUri
		{
			get
			{
				return "/WordsLive;component/Artwork/Small_Source.png";
			}
		}

		public SongNodeSource(SongNodeRoot root, SongSource source) : base(root)
		{
			this.source = source;
			this.Title = WordsLive.Resources.Resource.eMetadataSourceTitle;
		}

		public string Songbook
		{
			get
			{
				return source.Songbook;
			}
			set
			{
				EditorDocument.OnChangingTryMerge(this, "Songbook", source.Songbook, value);
				source.Songbook = value;
				OnNotifyPropertyChanged("Songbook");
			}
		}

		public int Number
		{
			get
			{
				return source.Number;
			}
			set
			{
				EditorDocument.OnChangingTryMerge(this, "Number", source.Number, value);
				source.Number = value;
				OnNotifyPropertyChanged("Number");
			}
		}

		public int FontSize
		{
			get
			{
				return Root.Song.Formatting.SourceText.Size;
			}
			set
			{
				DefaultChangeFactory.OnChanging(this, "FontSize", Root.Song.Formatting.SourceText.Size, value);
				Root.Song.Formatting.SourceText.Size = value;
				OnNotifyPropertyChanged("FontSize");
			}
		}
	}
}
