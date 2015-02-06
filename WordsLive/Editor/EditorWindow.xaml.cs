/*
 * WordsLive - worship projection software
 * Copyright (c) 2015 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WordsLive.Core;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.IO;
using WordsLive.Core.Songs.Storage;
using WordsLive.Resources;
using WordsLive.Songs;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class EditorWindow : Window, INotifyPropertyChanged
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
					OnPropertyChanged("ShowChords");
				}
			}
		}

		public bool FontSizeEnabled
		{
			get
			{
				var doc = Tabs.SelectedItem as EditorDocument;
				if (doc == null)
					return false;

				var element = doc.Grid.SelectedElement as ISongElementWithSize;
				return element != null;
			}
		}

		public EditorWindow()
		{
			InitializeComponent();
			this.DataContext = this;
			Tabs.DataContext = openDocuments;

			Tabs.SelectionChanged += (sender, args) => OnPropertyChanged("FontSizeEnabled");
		}

		public EditorDocument CheckSongOpened(Song song)
		{
			foreach (var doc in openDocuments)
			{
				if (doc.Song.Uri != null && doc.Song.Uri == song.Uri)
					return doc;
			}
			return null;
		}

		public void LoadOrImport(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			uri = DataManager.Songs.TryRewriteUri(uri);

			string ext = uri.GetExtension();

			Song song = null;

			try
			{
				if (ext == ".ppl")
				{
					song = new Song(uri, new PowerpraiseSongReader());
				}
				else if (ext == ".sng")
				{
					song = new Song(uri, new SongBeamerSongReader());
				}
				else if (ext == ".chopro" || ext == ".cho" || ext == ".pro")
				{
					song = new Song(uri, new ChordProSongReader());
				}
				else if (ext == ".usr")
				{
					song = new Song(uri, new CcliUsrSongReader());
				}
				else if (ext == ".txt")
				{
					song = new Song(uri, new CcliTxtSongReader());
				}
				else if (ext == ".xml")
				{
					song = new Song(uri, new OpenLyricsSongReader());
				}
				else if (ext == "") // OpenSong songs have no file extension
				{
					song = new Song(uri, new OpenSongSongReader());
				}
				else
				{
					throw new NotSupportedException("Song format is not supported.");
				}
			}
			catch(Exception e)
			{
				Controller.ShowEditorWindow();
				MessageBox.Show(String.Format(Resource.eMsgCouldNotOpenSong, uri.FormatLocal(), e.Message), Resource.dialogError, MessageBoxButton.OK, MessageBoxImage.Error);
			}

			if (song != null)
			{
				Load(song);
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

			opened.Grid.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "SelectedElement")
					OnPropertyChanged("FontSizeEnabled");
			};

			Tabs.SelectedItem = opened;
		}

		private void OpenSong()
		{
			// TODO: localize
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".ppl";
			dlg.Filter = "Powerpraise-Lied|*.ppl|SongBeamer-Lied|*.sng|ChordPro-Datei|*.chopro;*.cho;*.pro|SongSelect-Datei|*.usr;*.txt|OpenLyrics-Lied|*.xml";
			dlg.InitialDirectory = Properties.Settings.Default.LastSongDirectory;

			if (dlg.ShowDialog() == true)
			{
				Properties.Settings.Default.LastSongDirectory = Path.GetDirectoryName(dlg.FileName);
				LoadOrImport(new Uri(dlg.FileName));
			}
		}

		private void SaveSong(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (song.Uri == null || song.IsImported)
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

			SaveFilenameDialog dlg = new SaveFilenameDialog(song.Title);
			dlg.Owner = this;

			if (dlg.ShowDialog() == true)
			{
				song.Save(new Uri("song:///" + dlg.Filename));
			}
		}

		private void ExportSong(Song song)
		{
			// TODO: localize
			var dlg = new Microsoft.Win32.SaveFileDialog();
			string[] exts = { ".ppl", ".xml", ".html" };
			dlg.DefaultExt = exts[0];
			dlg.Filter = "Powerpraise-Lied|*.ppl|OpenLyrics-Lied|*.xml|HTML-Dokument|*.html"; // must be same order as exts
			dlg.Title = Resource.eMenuExportSong;
			dlg.InitialDirectory = Properties.Settings.Default.LastSongDirectory;

			if (song.Uri == null)
			{
				dlg.FileName = song.Title;
			}
			else
			{
				dlg.FileName = Path.GetFileNameWithoutExtension(Uri.UnescapeDataString(song.Uri.Segments.Last()));
			}

			if (dlg.ShowDialog() == true)
			{
				Properties.Settings.Default.LastSongDirectory = Path.GetDirectoryName(dlg.FileName);

				string path = dlg.FileName;
				string ext = Path.GetExtension(path).ToLower();

				if (!exts.Contains(ext))
				{
					ext = exts[dlg.FilterIndex - 1];
					path = path + ext;
				}

				if (ext == ".html")
				{
					song.Export(new Uri(path), new HtmlSongWriter());
				}
				else if (ext == ".xml")
				{
					song.Export(new Uri(path), new OpenLyricsSongWriter());
				}
				else if (ext == ".ppl")
				{
					song.Export(new Uri(path), new PowerpraiseSongWriter());
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
			song.Title = Resource.eNewSongTitle;
			Load(song);
		}

		private bool CloseSong(EditorDocument doc)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");

			bool doSave = false;

			if (doc.Song.IsModified)
			{
				var res = MessageBox.Show(String.Format(Resource.eMsgSaveSongChanges, doc.Song.Title), Resource.eMsgSaveSongChangesTitle, MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
				{
					return false;
				}

				if (res == MessageBoxResult.Yes)
				{
					doSave = true;
					SaveSong(doc.Song);
				}
			}

			bool didSave = !doc.Song.IsModified;

			// it might still be modified when saving was cancelled
			if (!doSave || didSave)
			{
				openDocuments.Remove(doc);
				doc.Grid.Cleanup();
				return true;
			}
			else
			{
				return false;
			}
		}

		private void Tabs_Drop(object sender, DragEventArgs e)
		{
			try
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
			catch (Exception ex)
			{
				Controller.ShowUnhandledException(ex, false);
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
			else if (e.Command == CustomCommands.ImportFromClipboard)
			{
				e.CanExecute = false;
				if (Clipboard.ContainsText())
				{
					string text = Clipboard.GetText();
					var firstNewline = text.IndexOf('\n');
					if (firstNewline > 0 && text.Length >= firstNewline + 1)
					{
						var next = text[firstNewline + 1];
						e.CanExecute = (next == '\n' || next == '\r') && text.Contains("CCLI");
					}
				}
			}
			else if (e.Command == ApplicationCommands.Close || e.Command == CustomCommands.SongSettings)
			{
				e.CanExecute = doc != null;
			}
			else if (e.Command == CustomCommands.ViewCurrent)
			{
				// deactivate this button if the current song is not yet saved to a file or it's not currently active
				e.CanExecute = (doc != null && doc.Song.Uri != null && Controller.ActiveMedia != null &&
					Controller.ActiveMedia.Uri == doc.Song.Uri);
			}
			else if (e.Command == CustomCommands.EditChords)
			{
				e.CanExecute = doc != null;
			}
			else if (e.Command == CustomCommands.AddMedia)
			{
				e.CanExecute = doc != null && !doc.Song.IsImported && doc.Song.Uri != null;
			}
			else if (e.Command == CustomCommands.SearchSongSelect)
			{
				string searchString = null;
				if (e.Parameter is string)
				{
					searchString = (string)e.Parameter;
				}
				else
				{
					int? searchNum = e.Parameter as int?;
					if (searchNum.HasValue)
						searchString = searchNum.ToString();
				}

				e.CanExecute = !String.IsNullOrWhiteSpace(searchString);
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
			else if (e.Command == CustomCommands.ImportFromClipboard)
			{
				try
				{
					var song = new Song(null, new ClipboardUriResolver(), new CcliTxtSongReader());
					Load(song);
				}
				catch
				{
					MessageBox.Show(Resource.eMsgCouldNotImportFromClipboard);
				}
			}
			else if (e.Command == ApplicationCommands.Close)
			{
				CloseSong(doc);
			}
			else if (e.Command == CustomCommands.ViewCurrent)
			{
				SaveSong(doc.Song);
				Controller.ReloadActiveMedia();
				Controller.FocusMainWindow(true);
			}
			else if (e.Command == CustomCommands.SwitchWindow)
			{
				if (openDocuments.Count == 0)
					this.Close();

				Controller.FocusMainWindow(true);
			}
			else if (e.Command == CustomCommands.SongSettings)
			{
				var win = new SongSettingsWindow(doc.Song.Formatting.Clone() as SongFormatting);
				win.Owner = this;
				if (win.ShowDialog() == true)
				{
					if (win.Formatting.SingleFontSize && !doc.Song.CheckSingleFontSize())
					{
						var res = MessageBox.Show(Resource.eMsgSingleFontSize, Resource.eMsgSingleFontSizeTitle, MessageBoxButton.YesNo);
						if (res == MessageBoxResult.No)
						{
							win.Formatting.SingleFontSize = false;
						}
					}
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
				Controller.AddToPortfolio(doc.Song.Uri);
			}
			else if (e.Command == ApplicationCommands.Find)
			{
				Controller.ShowSongList();
			}
			else if (e.Command == CustomCommands.SearchSongSelect)
			{
				string searchString = null;
				if (e.Parameter is string)
				{
					searchString = (string)e.Parameter;
				}
				else
				{
					int? searchNum = e.Parameter as int?;
					if (searchNum.HasValue)
						searchString = searchNum.ToString();
				}

				if (!String.IsNullOrWhiteSpace(searchString))
					new Uri(String.Format(Resource.eSongSelectSearchString, searchString)).OpenInBrowser();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// private class to import directly from clipboard
		private class ClipboardUriResolver : SongUriResolver
		{
			public override Stream Get(Uri uri)
			{
				if (Clipboard.ContainsText())
				{
					return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Clipboard.GetText()));
				}
				else
				{
					throw new InvalidOperationException("No text in clipboard.");
				}
			}

			public override Task<Stream> GetAsync(Uri uri, CancellationToken cancellation)
			{
				return TaskHelpers.FromResult(Get(uri));
			}

			public override FileTransaction Put(Uri uri)
			{
				throw new NotImplementedException();
			}
		}
	}
}
