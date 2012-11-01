using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WordsLive.Core;
using WordsLive.Core.Songs;
using WordsLive.Resources;
using WordsLive.Core.Data;

namespace WordsLive.Songs
{
	public partial class SongListWindow : Window
	{
		private SongFilter filter;
		private SongDataProvider provider;
		ObservableCollection<SongData> list = new ObservableCollection<SongData>();

		public SongListWindow()
		{
			InitializeComponent();

			provider = new LocalSongDataProvider(MediaManager.SongsDirectory); // TODO: use factory or something
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
			return filter.Matches((SongData)item);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			songListView.DataContext = this.list;
			songListView.Items.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
			filter = new SongFilter();
			filterGroupBox.DataContext = filter;

			new Action(LoadSongs).BeginInvoke(null, null);
		}

		private void LoadSongs()
		{
			foreach (var song in provider.All())
			{
				this.Dispatcher.BeginInvoke(new Action<SongData>(this.list.Add), song);
				this.Dispatcher.BeginInvoke(new Action<bool>(LoadUpdateUI), System.Windows.Threading.DispatcherPriority.Normal, false);
			}

			this.Dispatcher.BeginInvoke(new Action<bool>(LoadUpdateUI), System.Windows.Threading.DispatcherPriority.Normal, true);
		}

		private void LoadUpdateUI(bool finished)
		{
			if (finished)
				this.labelStatus.Content = String.Format(Resource.slFinishedLoadingN, this.list.Count);
			else
				this.labelStatus.Content = String.Format(Resource.slLoadingN, this.list.Count);
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

			SongData song = (SongData)songListView.ItemContainerGenerator.ItemFromContainer(dragItem);

			// Initialize the drag & drop operation
			DataObject dragData = new DataObject(DataFormats.FileDrop, new string[] { provider.GetFullPath(song) });
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

			var song = (SongData)songListView.ItemContainerGenerator.ItemFromContainer(sender as ListViewItem);
			Controller.AddToPortfolio(provider.GetFullPath(song));
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
				var song = (SongData)songListView.SelectedItem;
				Controller.AddToPortfolio(provider.GetFullPath(song));
			}
			else if (e.Command == ApplicationCommands.Close)
			{
				this.Close();
			}
		}
	}
}
