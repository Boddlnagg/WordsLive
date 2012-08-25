using Words.Core.Songs;

namespace Words.Editor
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
				return "/Words;component/Artwork/Small_Source.png";
			}
		}

		public SongNodeSource(SongNodeRoot root, SongSource source) : base(root)
		{
			this.source = source;
			this.Title = Words.Resources.Resource.eMetadataSourceTitle;
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
	}
}
