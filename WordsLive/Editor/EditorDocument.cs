using WordsLive.Core.Songs;

namespace WordsLive.Editor
{
	public class EditorDocument
	{
		public EditorDocument(Song song, EditorWindow parent)
		{
			Song = song;

			Grid = new EditorGrid(song, parent);
			Grid.PreviewControl.ShowChords = parent.ShowChords;

			Song.IsUndoEnabled = true;
		}

		public Song Song
		{
			get;
			private set;
		}

		public EditorGrid Grid
		{
			get;
			private set;
		}
	}
}
