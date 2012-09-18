﻿using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WordsLive.Core.Songs;
using System.Windows.Documents;

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

		/// <summary>
		/// Creates a new instance of <see cref="Song"/> from the template file (the SongTemplateFile application setting).
		/// This is used for creating new (empty) songs and for importing songs.
		/// </summary>
		/// <returns>The created song.</returns>
		private Song CreateSongFromTemplate()
		{
			// init song template file
			var template = Properties.Settings.Default.SongTemplateFile;

			if (string.IsNullOrEmpty(template) || !File.Exists(template))
			{
				// fall back to standard template in data directory
				template = Path.Combine("Data", "Standard.ppl");
			}

			return new Song(template) { SongTitle = WordsLive.Resources.Resource.eNewSongTitle };
		}

		public void LoadOrImport(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			var file = new FileInfo(filename);

			if (file.Extension == ".ppl")
			{
				Load(filename, new Song(filename), false);
			}
			else if (file.Extension == ".sng")
			{
				var song = CreateSongFromTemplate();
				SongBeamerImport.Import(song, filename);
				Load(filename, song, true);
			}
			else
			{
				Controller.ShowEditorWindow();
				MessageBox.Show(String.Format(WordsLive.Resources.Resource.eMsgCouldNotOpenSong, file), WordsLive.Resources.Resource.dialogError, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Load(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			Load(null, song, false);
		}

		private void Load(string file, Song song, bool imported)
		{
			var opened = CheckSongOpened(file, song);
			if (opened == null)
			{
				opened = new EditorDocument(file, song, imported, this);
				openDocuments.Add(opened);
			}

			Tabs.SelectedItem = opened;
		}

		private void OpenSong()
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".ppl";
			dlg.Filter = "Powerpraise-Lied|*.ppl|SongBeamer-Lied|*.sng";

			if (dlg.ShowDialog() == true)
			{
				LoadOrImport(dlg.FileName);
			}
		}

		private void SaveSong(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");
			
			if (doc.File == null || doc.File.Extension.ToLower() != ".ppl")
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
				dlg.FileName =  Path.GetFileNameWithoutExtension(doc.File.Name);
			}

			if (dlg.ShowDialog() == true)
			{
				doc.SaveAs(dlg.FileName);
			}
		}

		private void NewSong()
		{
			Load(CreateSongFromTemplate());
		}

		private bool CloseSong(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");

			if (doc.IsModified)
			{
				var res = MessageBox.Show(String.Format(WordsLive.Resources.Resource.eMsgSaveSongChanges, doc.Song.SongTitle), WordsLive.Resources.Resource.eMsgSaveSongChangesTitle, MessageBoxButton.YesNoCancel);
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
				bg = doc.Song.Backgrounds[doc.Song.FirstSlide != null ? doc.Song.FirstSlide.BackgroundIndex : 0];
			else
			{
				MessageBox.Show(WordsLive.Resources.Resource.eMsgSelectElement);
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
					LoadOrImport(file);
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
			else if (e.Command == CustomCommands.AddMedia)
			{
				e.CanExecute = doc != null && !doc.IsImported && doc.File != null;
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
			else if (e.Command == CustomCommands.AddMedia)
			{
				Controller.AddToPortfolio(doc.File.FullName);
			}
			else if (e.Command == CustomCommands.ShowSonglist)
			{
				Controller.ShowSongList();
			}
		}
	}
}
