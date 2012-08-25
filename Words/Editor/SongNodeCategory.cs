
namespace Words.Editor
{
	public class SongNodeCategory : SongNodeMetadata
	{
		public SongNodeCategory(SongNodeRoot root) : base(root)
		{
			this.Title = Words.Resources.Resource.eMetadataCategoryTitle;
			this.Text = root.Song.Category;
		}

		protected override void UpdateSource(string value)
		{
			Root.Song.Category = value;
		}

		public override string IconUri
		{
			get
			{
				return "/Words;component/Artwork/Small_Category.png";
			}
		}
	}
}
