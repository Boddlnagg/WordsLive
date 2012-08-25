
namespace Words.Editor
{
	public class SongNodeLanguage : SongNodeMetadata
	{
		public SongNodeLanguage(SongNodeRoot root) : base(root)
		{
			this.Title = Words.Resources.Resource.eMetadataLanguageTitle;
			this.Text = root.Song.Language;
		}

		protected override void UpdateSource(string value)
		{
			Root.Song.Language = value;
		}

		public override string IconUri
		{
			get { return "/Words;component/Artwork/Small_Language.png"; }
		}
	}
}
