using System;
using System.ComponentModel;
using System.IO;
using WordsLive.Core.Songs;
using WordsLive.Core.Data;

namespace WordsLive.Editor
{
	public class EditorDocument : INotifyPropertyChanged
	{
		public EditorDocument(string file, Song song, bool isImported, EditorWindow parent)
		{
			if (String.IsNullOrEmpty(file))
			{
				this.File = null;
			}
			else
			{
				this.File = new FileInfo(file);

				this.IsImported = isImported;
					
			}

			Song = song;

			Grid = new EditorGrid(song, parent);
			Grid.PreviewControl.ShowChords = parent.ShowChords;

			Song.IsUndoEnabled = true;

			Song.UndoManager.PropertyChanged += (sender, args) =>
			{
				if (Song.UndoManager.CanUndo)
				{
					IsModified = true;
				}
			};
		}

		//private string GetChangesetText(ChangeSet set)
		//{
		//    if (set.Changes.Count() == 0)
		//    {
		//        return set.Description;
		//    }
		//    else
		//    {
		//        string name;
		//        object target = set.Changes.First().Target;
		//        if (target is SongNodeRoot)
		//            name = "Lied";
		//        else if (target is SongNodePart)
		//            name = "Liedteil";
		//        else if (target is SongNodeSlide)
		//            name = "Folie";
		//        else if (target is SongNodeSource)
		//            name = "Quelle";
		//        else if (target is SongNodeMetadata)
		//            name = (target as SongNodeMetadata).Title;
		//        else
		//            throw new InvalidOperationException();

		//        return set.Description + " in " + name;
		//    }
		//}

		//TODO: move the following to model class, refactor save/load/import/export infrastructure

		private bool isModified;

		public bool IsModified
		{
			get
			{
				return isModified;
			}
			private set
			{
				isModified = value;
				OnPropertyChanged("IsModified");
			}
		}

		private bool isImported;

		public bool IsImported
		{
			get
			{
				return isImported;
			}
			private set
			{
				isImported = value;
				OnPropertyChanged("IsImported");

				if (value)
					IsModified = true;
			}
		}

		public void Save()
		{
			Song.Save();
			IsModified = false;
			IsImported = false;
		}

		public void SaveAs(string file)
		{
			Song.Save(file, DataManager.LocalFiles);
			IsModified = false;
			IsImported = false;
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

		private FileInfo file;

		public FileInfo File
		{
			get
			{
				return file;
			}
			protected set
			{
				file = value;
				OnPropertyChanged("File");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
