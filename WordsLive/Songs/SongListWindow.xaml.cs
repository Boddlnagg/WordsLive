/*
 * WordsLive - worship projection software
 * Copyright (c) 2012-2013 Patrick Reisert
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WordsLive.Core;
using WordsLive.Core.Songs.Storage;
using WordsLive.Resources;

namespace WordsLive.Songs
{
	public partial class SongListWindow : Window
	{
		private SongFilter filter;
		ObservableCollection<SongData> list = new ObservableCollection<SongData>();

		public SongListWindow()
		{
			InitializeComponent();
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
			filter.PropertyChanged += (s, a) =>
			{
				ICollectionView view = CollectionViewSource.GetDefaultView(songListView.ItemsSource);
				view.Filter = filter.IsEmpty ? null : new Predicate<object>(FilterCallback);
				songListView.SelectedIndex = 0;
			};
			filterGroupBox.DataContext = filter;

			new Action(LoadSongs).BeginInvoke(null, null);
		}

		private void LoadSongs()
		{
			foreach (var song in DataManager.Songs.All())
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
			IDataObject dragData = new SongDataObject(song);
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
			AddToPortfolio(song);
		}

		private void AddToPortfolio(SongData data)
		{
			Controller.AddToPortfolio(data.Uri);
			this.Topmost = true;
			Controller.FocusMainWindow();
			this.Topmost = false;
			this.Focus();
		}

		private void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			SongData data;
			if (e.Parameter as SongData != null)
				data = (SongData)e.Parameter;
			else
				data = (SongData)songListView.SelectedItem;

			if (e.Command == CustomCommands.AddMedia || e.Command == CustomCommands.OpenInEditor)
			{
				e.CanExecute = data != null;
			}
			else if (e.Command == ApplicationCommands.Delete)
			{
				e.CanExecute = data != null;
			}
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			SongData data;
			if (e.Parameter as SongData != null)
				data = (SongData)e.Parameter;
			else
				data = (SongData)songListView.SelectedItem;

			if (e.Command == CustomCommands.AddMedia)
			{
				AddToPortfolio(data);
			}
			else if (e.Command == CustomCommands.OpenInEditor)
			{
				var editor = Controller.ShowEditorWindow();
				editor.LoadOrImport(data.Uri);
			}
			else if (e.Command == ApplicationCommands.Delete)
			{
				var provider = DataManager.Songs;
				var result = MessageBox.Show(String.Format(Resource.slMsgDeleteSong, data.Title), Resource.slMsgDeleteSongTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					provider.Delete(data.Filename);
					list.Remove(data);
				}
			}
			else if (e.Command == ApplicationCommands.Close)
			{
				this.Close();
			}
		}
	}
}
