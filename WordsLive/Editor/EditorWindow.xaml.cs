using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WordsLive.Core;
using WordsLive.Core.Data;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.IO;
using WordsLive.Core.Songs.Storage;
using WordsLive.Resources;
using WordsLive.Songs;

namespace WordsLive.Editor
{
	public partial class EditorWindow : Window
	{
		ObservableCollection<EditorDocument> openDocuments = new ObservableCollection<EditorDocument>();

		bool showChords;

		public bool ShowChords
		{
			get
			{
				return showChords;
			}
			set
			{
				if (value != showChords)
				{
					showChords = value;
					foreach (var doc in openDocuments)
					{
						doc.Grid.PreviewControl.ShowChords = showChords;
					}
				}
			}
		}

		public EditorWindow()
		{
			InitializeComponent();
			this.DataContext = this;
			Tabs.DataContext = openDocuments;
		}

		public EditorDocument CheckSongOpened(Song song)
		{
			foreach (var doc in openDocuments)
			{
				if (doc.Song.File != null && doc.Song.File == song.File && doc.Song.DataProvider == song.DataProvider)
					return doc;
			}
			return null;
		}

		public void LoadOrImport(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			string ext = uri.GetExtension();

			try
			{ 
				if (ext == ".ppl")
				{
					var song = new Song(uri);
					song.Load();
					Load(song);
				}
				else if (ext == ".sng")
				{
					var song = new Song(uri, new SongBeamerSongReader());
					Load(song);
				}
				else if (ext == ".chopro" || ext == ".cho" || ext == ".pro")
				{
					var song = new Song(uri, new ChordProSongReader());
					Load(song);
				}
				else if (ext == "") // OpenSong songs have no file extension
				{
					var song = new Song(uri, new OpenSongSongReader());
					Load(song);
				}
				else
				{
					throw new NotSupportedException("Song format is not supported.");
				}
			}
			catch
			{
				Controller.ShowEditorWindow();
				MessageBox.Show(String.Format(Resource.eMsgCouldNotOpenSong, uri.FormatLocal()), Resource.dialogError, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Load(Song song)
		{
			var opened = CheckSongOpened(song);
			if (opened == null)
			{
				opened = new EditorDocument(song, this);
				openDocuments.Add(opened);
			}

			Tabs.SelectedItem = opened;
		}

		private void OpenSong()
		{
			// TODO: localize
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".ppl";
			dlg.Filter = "Powerpraise-Lied|*.ppl|SongBeamer-Lied|*.sng|ChordPro-Datei|*.chopro;*.cho;*.pro";

			if (dlg.ShowDialog() == true)
			{
				LoadOrImport(new Uri(dlg.FileName));
			}
		}

		private void SaveSong(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");
			
			if (song.File == null || song.IsImported)
			{
				SaveSongAs(song);
			}
			else
			{
				song.Save();
			}
		}

		private void SaveSongAs(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			SaveFilenameDialog dlg = new SaveFilenameDialog(song.SongTitle);
			dlg.Owner = this;

			if (dlg.ShowDialog() == true)
			{
				throw new NotImplementedException(); // TODO!!
				//song.Save(dlg.Filename, DataManager.Songs);
			}
		}

		private void ExportSong(Song song)
		{
			// TODO: localize
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			string[] exts = { ".ppl", ".html" };
			dlg.DefaultExt = exts[0];
			dlg.Filter = "Powerpraise-Lied|*.ppl|HTML-Dokument|*.html"; // must be same order as exts
			dlg.Title = Resource.eMenuExportSong;
			if (song.File == null)
			{
				dlg.FileName = song.SongTitle;
			}
			else
			{
				dlg.FileName =  Path.GetFileNameWithoutExtension(Path.GetFileName(song.File));
			}

			if (dlg.ShowDialog() == true)
			{
				string path = dlg.FileName;
				string ext = Path.GetExtension(path).ToLower();

				if (!exts.Contains(ext))
				{
					ext = exts[dlg.FilterIndex - 1];
					path = path + ext;
				}

				if (ext == ".html")
				{
					song.Export(path, DataManager.LocalFiles, new HtmlSongWriter());
				}
				else if (ext == ".ppl")
				{
					song.Export(path, DataManager.LocalFiles, new PowerpraiseSongWriter());
				}
				else
				{
					// TODO: add more formats
					throw new InvalidOperationException("Invalid extension " + ext + ". This should not happen.");
				}
				
			}
		}

		private void NewSong()
		{
			var song = Song.CreateFromTemplate();
			song.SongTitle = Resource.eNewSongTitle;
			Load(song);
		}

		private bool CloseSong(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");

			if (doc.Song.IsModified)
			{
				var res = MessageBox.Show(String.Format(Resource.eMsgSaveSongChanges, doc.Song.SongTitle), Resource.eMsgSaveSongChangesTitle, MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
				{
					return false;
				}

				if (res == MessageBoxResult.Yes)
				{
					SaveSong(doc.Song);
				}
			}

			openDocuments.Remove(doc);

			doc.Grid.Cleanup();

			return true;
		}

		// TODO: move method to EditorGrid
		private void ChooseBackground(EditorDocument doc)
		{
			SongBackground bg = null;

			var element = (ISongElement)doc.Grid.StructureTree.SelectedItem;

			if (element is SongSlide)
				bg = (element as SongSlide).Background;
			else if (element is SongPart)
				bg = (element as SongPart).Slides[0].Background;
			else if (element is Song)
				bg = element.Root.FirstSlide != null ? element.Root.FirstSlide.Background : element.Root.Backgrounds[0];
			else
			{
				MessageBox.Show(Resource.eMsgSelectElement);
				return;
			}

			var win = new ChooseBackgroundWindow(bg);
			win.Owner = this;
			win.ShowDialog();
			if (win.DialogResult.HasValue && win.DialogResult.Value)
			{
				if (element is Song)
				{
					var song = element as Song;

					song.SetBackground(win.ChosenBackground);

					// this needs to be called manually, because the preview can not listen to background changes
					// when the root node is selected
					doc.Grid.PreviewControl.Update();
				}
				else if (element is SongPart)
				{
					var part = element as SongPart;

					// only set background if it is different
					var bgs = part.Slides.Select(s => s.Background).Distinct();
					if (bgs.Count() != 1 || !bgs.First().Equals(win.ChosenBackground))
					{
						if (win.ChosenBackground.Type == SongBackgroundType.Video)
						{
							var res = MessageBox.Show(Resource.eMsgVideoBackgroundForElement, Resource.eMsgVideoBackgroundForElementTitle, MessageBoxButton.YesNo);
							if (res == MessageBoxResult.Yes)
								part.Root.SetBackground(win.ChosenBackground);
						}
						else if (part.Root.VideoBackground != null)
						{
							var res = MessageBox.Show(Resource.eMsgReplaceVideoBackground, Resource.eMsgReplaceVideoBackgroundTitle, MessageBoxButton.YesNo);
							if (res == MessageBoxResult.Yes)
								part.Root.SetBackground(win.ChosenBackground);
						}
						else
						{
							part.SetBackground(win.ChosenBackground);
						}
					}
				}
				else if (element is SongSlide)
				{
					var slide = element as SongSlide;
					if (!slide.Background.Equals(win.ChosenBackground))
					{
						if (win.ChosenBackground.Type == SongBackgroundType.Video)
						{
							var res = MessageBox.Show(Resource.eMsgVideoBackgroundForElement, Resource.eMsgVideoBackgroundForElementTitle, MessageBoxButton.YesNo);
							if (res == MessageBoxResult.Yes)
								slide.Root.SetBackground(win.ChosenBackground);
						}
						else if (slide.Root.VideoBackground != null)
						{
							var res = MessageBox.Show(Resource.eMsgReplaceVideoBackground, Resource.eMsgReplaceVideoBackgroundTitle, MessageBoxButton.YesNo);
							if (res == MessageBoxResult.Yes)
								slide.Root.SetBackground(win.ChosenBackground);
						}
						else
						{
							slide.SetBackground(win.ChosenBackground);
						}
					}
				}
			}
		}

		private void Tabs_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetData(SongDataObject.SongDataFormat) != null)
			{
				var song = (SongData)e.Data.GetData(SongDataObject.SongDataFormat);
				LoadOrImport(song.Uri);
			}
			else if (e.Data.GetData(DataFormats.FileDrop) != null)
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (var file in files)
				{
					LoadOrImport(new Uri(file));
				}
			}
		}

		private void Tabs_DragEnterOrOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetData(DataFormats.FileDrop) != null)
			{
				e.Effects = DragDropEffects.Copy;
				e.Handled = true;
			}
			else
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			this.Focus();
			while (openDocuments.Count > 0)
			{
				if (!CloseSong(openDocuments[0]))
				{
					e.Cancel = true;
					return;
				}
			}
		}

		private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			EditorDocument doc;

			if (e.Parameter != null && e.Parameter is EditorDocument)
				doc = e.Parameter as EditorDocument;
			else 
				doc = Tabs != null ? (Tabs.SelectedItem as EditorDocument) : null;

			if (e.Command == ApplicationCommands.Save)
			{
				e.CanExecute = doc != null && doc.Song.IsModified;
			}
			else if (e.Command == ApplicationCommands.SaveAs)
			{
				e.CanExecute = doc != null;
			}
			else if (e.Command == CustomCommands.Export)
			{
				e.CanExecute = doc != null;
			}
			else if (e.Command == ApplicationCommands.Close || e.Command == CustomCommands.ChooseBackground || e.Command == CustomCommands.SongSettings)
			{
				e.CanExecute = doc != null;
			}
			else if (e.Command == CustomCommands.ViewCurrent)
			{
				// deactivate this button if the current song is not yet saved to a file or it's not currently active
				e.CanExecute = (doc != null && doc.Song.File != null && Controller.ActiveMedia != null &&
					Controller.ActiveMedia.File == doc.Song.File && Controller.ActiveMedia.DataProvider == doc.Song.DataProvider);
			}
			else if (e.Command == CustomCommands.EditChords)
			{
				e.CanExecute = doc != null;
			}
			else if (e.Command == CustomCommands.AddMedia)
			{
				e.CanExecute = doc != null && !doc.Song.IsImported && doc.Song.File != null;
			}
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			EditorDocument doc;

			if (e.Parameter != null && e.Parameter is EditorDocument)
				doc = e.Parameter as EditorDocument;
			else
				doc = Tabs != null ? (Tabs.SelectedItem as EditorDocument) : null;

			if (e.Command == ApplicationCommands.New)
			{
				NewSong();
			}
			else if (e.Command == ApplicationCommands.Open)
			{
				OpenSong();
			}
			else if (e.Command == ApplicationCommands.Save)
			{
				SaveSong(doc.Song);
			}
			else if (e.Command == ApplicationCommands.SaveAs)
			{
				SaveSongAs(doc.Song);
			}
			else if (e.Command == CustomCommands.Export)
			{
				ExportSong(doc.Song);
			}
			else if (e.Command == ApplicationCommands.Close)
			{
				CloseSong(doc);
			}
			else if (e.Command == CustomCommands.ViewCurrent)
			{
				SaveSong(doc.Song);
				Controller.ReloadActiveMedia();
				Controller.FocusMainWindow();
			}
			else if (e.Command == CustomCommands.ChooseBackground)
			{
				ChooseBackground(doc);
			}
			else if (e.Command == CustomCommands.SwitchWindow)
			{
				if (openDocuments.Count == 0)
					this.Close();

				Controller.FocusMainWindow();
			}
			else if (e.Command == CustomCommands.SongSettings)
			{
				var win = new SongSettingsWindow(doc.Song.Formatting.Clone() as SongFormatting);
				if (win.ShowDialog() == true)
				{
					doc.Song.Formatting = win.Formatting;
				}
			}
			else if (e.Command == CustomCommands.EditChords)
			{
				var win = new EditChordsWindow(doc.Song);
				win.Owner = this;
				win.ShowDialog();
			}
			else if (e.Command == CustomCommands.AddMedia)
			{
				Controller.AddToPortfolio(doc.Song.File, doc.Song.DataProvider);
			}
			else if (e.Command == CustomCommands.ShowSonglist)
			{
				Controller.ShowSongList();
			}
		}
	}
}
