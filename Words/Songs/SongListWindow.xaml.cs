using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Words.Core;
using Words.Core.Songs;
using Words.Resources;

namespace Words.Songs
{
	public partial class SongListWindow : Window
	{
		private SongFilter filter;
		ObservableCollection<SongWrapper> songList = new ObservableCollection<SongWrapper>();

		public SongListWindow()
		{
			InitializeComponent();
		}

		private void filterButton_Click(object sender, RoutedEventArgs e)
		{
			// work-around for IsDefault-Button data-binding bug (see http://berndhengelein.de/2009/03/wpf-databinding-in-verbindung-mit-einem-defaultbutton/)
			TextBox focusedTextBox = Keyboard.FocusedElement as TextBox;
			if (null != focusedTextBox)
			{
				BindingExpression bindingExpression = focusedTextBox.GetBindingExpression(TextBox.TextProperty);
				if (null != bindingExpression)
				{
					bindingExpression.UpdateSource();
				}
			}

			ICollectionView view = CollectionViewSource.GetDefaultView(songListView.ItemsSource);
			view.Filter = filter.IsEmpty ? null : new Predicate<object>(FilterCallback);
		}

		private bool FilterCallback(object item)
		{
			return filter.Matches(((SongWrapper)item).Song);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			songListView.DataContext = this.songList;
			songListView.Items.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
			filter = new SongFilter();
			filterGroupBox.DataContext = filter;

			new Action(LoadSongs).BeginInvoke(null, null);
		}

		private void LoadSongs()
		{
			foreach (string file in Directory.GetFiles(MediaManager.SongsDirectory))
			{
				try
				{
					SongWrapper sm = new SongWrapper(new Song(file), file);
					this.Dispatcher.BeginInvoke(new Action<SongWrapper>(this.songList.Add), sm);
					this.Dispatcher.BeginInvoke(new Action<bool>(LoadUpdateUI), System.Windows.Threading.DispatcherPriority.Normal, false);
				}
				catch { }
			}

			this.Dispatcher.BeginInvoke(new Action<bool>(LoadUpdateUI), System.Windows.Threading.DispatcherPriority.Normal, true);
		}

		private void LoadUpdateUI(bool finished)
		{
			if (finished)
				this.labelStatus.Content = String.Format(Resource.slFinishedLoadingN, this.songList.Count);
			else
				this.labelStatus.Content = String.Format(Resource.slLoadingN, this.songList.Count);
		}

		ListViewItem dragItem;

		private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			dragItem = sender as ListViewItem;
		}

		private void List_MouseMove(object sender, MouseEventArgs e)
		{
			if (dragItem == null)
				return;

			SongWrapper song = (SongWrapper)songListView.ItemContainerGenerator.ItemFromContainer(dragItem);

			// Initialize the drag & drop operation
			DataObject dragData = new DataObject(DataFormats.FileDrop, new string[] { song.Path });
			DragDrop.DoDragDrop(dragItem, dragData, DragDropEffects.Copy);
			dragItem = null;
		}

		private void songListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			dragItem = null;
		}

		private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			var song = (SongWrapper)songListView.ItemContainerGenerator.ItemFromContainer(sender as ListViewItem);
			Controller.AddToPortfolio(song.Path);
		}

		private void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.AddMedia)
			{
				e.CanExecute = songListView.SelectedItem != null;
			}
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.AddMedia)
			{
				var song = (SongWrapper)songListView.SelectedItem;
				Controller.AddToPortfolio(song.Path);
			}
			else if (e.Command == ApplicationCommands.Close)
			{
				this.Close();
			}
		}
	}
}
