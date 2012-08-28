
namespace Words.Editor
{
	public class SongNodeLanguage : SongNode
	{
		public SongNodeLanguage(SongNodeRoot root) : base(root)
		{
			this.Title = Words.Resources.Resource.eMetadataLanguageTitle;
		}

		public string Language
		{
			get
			{
				return Root.Song.Language;
			}
			set
			{
				EditorDocument.OnChangingTryMerge(this, "Language", Root.Song.Language, value);
				Root.Song.Language = value;
				OnNotifyPropertyChanged("Language");
			}
		}

		public string TranslationLanguage
		{
			get
			{
				return Root.Song.TranslationLanguage;
			}
			set
			{
				EditorDocument.OnChangingTryMerge(this, "TranslationLanguage", Root.Song.TranslationLanguage, value);
				Root.Song.TranslationLanguage = value;
				OnNotifyPropertyChanged("TranslationLanguage");
			}
		}

		public override string IconUri
		{
			get { return "/Words;component/Artwork/Small_Language.png"; }
		}
	}
}
