
namespace Words.Editor
{
	public abstract class SongNodeMetadata : SongNode
	{
		public SongNodeMetadata(SongNodeRoot root) : base(root)
		{ }

		private string text;
		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				EditorDocument.OnChangingTryMerge(this, "Text", text, value);
				text = value;
				UpdateSource(this.Text);
				OnNotifyPropertyChanged("Text");
			}
		}

		protected abstract void UpdateSource(string value);
	}
}
