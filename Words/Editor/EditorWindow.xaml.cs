using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Words.Core.Songs;
using System.Windows.Documents;

namespace Words.Editor
{
	public partial class EditorWindow : Window
	{
		ObservableCollection<EditorDocument> openDocuments = new ObservableCollection<EditorDocument>();

		public EditorWindow()
		{
			InitializeComponent();
			Tabs.DataContext = openDocuments;
		}

		public EditorDocument CheckSongOpened(string file, Song song)
		{
			FileInfo f = String.IsNullOrEmpty(file) ? null : new FileInfo(file);

			foreach (var doc in openDocuments)
			{
				if (f != null && doc.File != null && doc.File.FullName == f.FullName)
					return doc;

				if (song != null && doc.Song == song)
					return doc;
			}
			return null;
		}

		public void Load(string file)
		{
			Load(file, new Song(file));
		}

		public void Load(Song song)
		{
			Load(null, song);
		}

		public void Load(string file, Song song)
		{
			var opened = CheckSongOpened(file, song);
			if (opened == null)
			{
				opened = new EditorDocument(file, song, this);
				openDocuments.Add(opened);
			}

			Tabs.SelectedItem = opened;
		}

		private void OpenSong()
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".ppl";
			dlg.Filter = "Powerpraise-Lied|*.ppl";

			if (dlg.ShowDialog() == true)
			{
				Load(dlg.FileName);
			}
		}

		private void SaveSong(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");
			
			if (doc.File == null)
			{
				SaveSongAs(doc);
			}
			else
			{
				doc.Save();
			}
		}

		private void SaveSongAs(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.DefaultExt = ".ppl";
			dlg.Filter = "Powerpraise-Lied|*.ppl";
			if (doc.File == null)
			{
				dlg.FileName = doc.Song.SongTitle;
			}
			else
			{
				dlg.FileName = doc.File.Name;
			}

			if (dlg.ShowDialog() == true)
			{
				doc.SaveAs(dlg.FileName);
			}
		}

		private void NewSong()
		{
			var template = Properties.Settings.Default.SongTemplateFile;

			if (string.IsNullOrEmpty(template) || !File.Exists(template))
			{
				// fall back to standard template in data directory
				template = Path.Combine("Data", "Standard.ppl");
			}

			Load(new Song(template) { SongTitle = "Neues Lied" });
		}

		private bool CloseSong(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");

			if (doc.IsModified)
			{
				var res = MessageBox.Show("Das Lied \"" + doc.Song.SongTitle + "\" enthält möglicherweise ungespeicherte Änderungen. Wollen Sie es speichern?", "Änderungen speichern?", MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
				{
					return false;
				}

				if (res == MessageBoxResult.Yes)
				{
					SaveSong(doc);
				}
			}

			openDocuments.Remove(doc);

			doc.Grid.Cleanup();

			return true;
		}

		private void ChooseBackground(EditorDocument doc)
		{
			SongBackground bg = null;
			if (doc.Grid.StructureTree.SelectedItem is SongNodeSlide)
				bg = doc.Song.Backgrounds[(doc.Grid.StructureTree.SelectedItem as SongNodeSlide).BackgroundIndex];
			else if (doc.Grid.StructureTree.SelectedItem is SongNodePart)
				bg = doc.Song.Backgrounds[(doc.Grid.StructureTree.SelectedItem as SongNodePart).Children[0].BackgroundIndex];
			else if (doc.Grid.StructureTree.SelectedItem is SongNodeRoot)
				bg = doc.Song.Backgrounds[0];
			else
			{
				MessageBox.Show("Bitte wählen Sie entweder das Lied, einen Liedteil oder eine Folie aus.");
				return;
			}

			var win = new ChooseBackgroundWindow(bg);
			win.Owner = this;
			win.ShowDialog();
			if (win.DialogResult.HasValue && win.DialogResult.Value)
			{
				if (doc.Grid.StructureTree.SelectedItem is SongNodeRoot)
				{
					var song = doc.Grid.StructureTree.SelectedItem as SongNodeRoot;
					song.SetBackground(win.ChosenBackground);

					// this needs to be called manually, because the preview can not listen to background changes
					// when the root node is selected
					doc.Grid.PreviewControl.Update();
				}
				else if (doc.Grid.StructureTree.SelectedItem is SongNodePart)
				{
					var part = doc.Grid.StructureTree.SelectedItem as SongNodePart;
					part.SetBackground(win.ChosenBackground);
				}
				else if (doc.Grid.StructureTree.SelectedItem is SongNodeSlide)
				{
					var slide = doc.Grid.StructureTree.SelectedItem as SongNodeSlide;
					slide.SetBackground(win.ChosenBackground);
				}
			}
		}

		private void Tabs_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetData(DataFormats.FileDrop) != null)
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (var file in files)
				{
					Load(file);
				}
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

			if (e.Command == ApplicationCommands.Undo)
			{
				e.CanExecute = doc != null && doc.CanUndo;
			}
			else if (e.Command == ApplicationCommands.Redo)
			{
				e.CanExecute = doc != null && doc.CanRedo;
			}
			else if (e.Command == ApplicationCommands.Save)
			{
				e.CanExecute = doc != null && doc.IsModified;
			}
			else if (e.Command == ApplicationCommands.SaveAs)
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
				e.CanExecute = (doc != null && doc.File != null && Controller.ActiveMedia != null && Controller.ActiveMedia.File == doc.File.FullName);
			}
			else if (e.Command == CustomCommands.EditChords)
			{
				e.CanExecute = doc != null;
			}
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			EditorDocument doc;

			if (e.Parameter != null && e.Parameter is EditorDocument)
				doc = e.Parameter as EditorDocument;
			else
				doc = Tabs != null ? (Tabs.SelectedItem as EditorDocument) : null;

			if (e.Command == ApplicationCommands.Undo)
			{
				doc.Undo();
			}
			else if (e.Command == ApplicationCommands.Redo)
			{
				doc.Redo();
			}
			else if (e.Command == ApplicationCommands.New)
			{
				NewSong();
			}
			else if (e.Command == ApplicationCommands.Open)
			{
				OpenSong();
			}
			else if (e.Command == ApplicationCommands.Save)
			{
				SaveSong(doc);
			}
			else if (e.Command == ApplicationCommands.SaveAs)
			{
				SaveSongAs(doc);
			}
			else if (e.Command == ApplicationCommands.Close)
			{
				CloseSong(doc);
			}
			else if (e.Command == CustomCommands.ViewCurrent)
			{
				SaveSong(doc);
				Controller.ReloadActiveMedia();
				Controller.ShowMainWindow();
			}
			else if (e.Command == CustomCommands.ChooseBackground)
			{
				ChooseBackground(doc);
			}
			else if (e.Command == CustomCommands.SwitchWindow)
			{
				if (openDocuments.Count == 0)
					this.Close();

				Controller.ShowMainWindow();
			}
			else if (e.Command == CustomCommands.SongSettings)
			{
				var win = new SongSettingsWindow(doc.Song.Formatting);
				if (win.ShowDialog() == true)
				{
					doc.UpdateFormatting(win.Formatting);
				}
			}
			else if (e.Command == CustomCommands.EditChords)
			{
				var win = new EditChordsWindow(doc.Grid.Node);
				win.Owner = this;
				win.ShowDialog();
				doc.Grid.PreviewControl.UpdateStyle();
				
			}
		}
	}
}
