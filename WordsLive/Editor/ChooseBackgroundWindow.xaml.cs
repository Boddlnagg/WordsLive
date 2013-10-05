/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WordsLive.Core;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.Storage;
using WordsLive.Resources;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class ChooseBackgroundWindow : Window, INotifyPropertyChanged
	{
		public SongBackground ChosenBackground
		{
			get;
			private set;
		}

		private bool useImage;

		public bool UseImage
		{
			get
			{
				return useImage;
			}
			set
			{
				useImage = value;
				OnNotifyPropertyChanged("UseImage");
				OnNotifyPropertyChanged("UseColor");
			}
		}

		public bool UseColor
		{
			get
			{
				return !useImage;
			}
			set
			{
				useImage = !value;
				OnNotifyPropertyChanged("UseImage");
				OnNotifyPropertyChanged("UseColor");
			}
		}

		private bool applyToAllSlides;

		public bool ApplyToAllSlides
		{
			get
			{
				return applyToAllSlides;
			}
			set
			{
				if (canOnlyApplyToAllSlides && !value)
					throw new InvalidOperationException("Can't enable property 'ApplyToAllSlides'");

				applyToAllSlides = value;
				OnNotifyPropertyChanged("ApplyToAllSlides");
			}
		}

		private bool canOnlyApplyToAllSlides;

		public bool CanOnlyApplyToAllSlides
		{
			get
			{
				return canOnlyApplyToAllSlides;
			}
			private set
			{
				canOnlyApplyToAllSlides = value;

				if (value)
					ApplyToAllSlides = true;
			}
		}

		// for data-binding
		public bool NotCanOnlyApplyToAllSlides
		{
			get
			{
				return !CanOnlyApplyToAllSlides;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string property)
		{ 
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}


		public ChooseBackgroundWindow(SongBackground background, bool canOnlyApplyToAllSlides)
		{
			InitializeComponent();

			this.CanOnlyApplyToAllSlides = canOnlyApplyToAllSlides;

			this.DataContext = this;

			UseImage = background.IsFile;

			directoryView.DataContext = new BackgroundStorageDirectory[] { DataManager.Backgrounds.Root };

			string selectPath = String.Empty;

			if (background.IsFile)
			{
				var file = DataManager.Backgrounds.GetFile(background);
				if (file.Exists)
				{
					selectPath = background.FilePath;
				}
				else
				{
					MessageBox.Show(this, Resource.cbMsgNotFound, Resource.cbMsgNotFoundTitle);
					UseColor = true;
					ColorPicker.SelectedColor = Colors.Black;
				}
			}
			else
			{
				ColorPicker.SelectedColor = Color.FromRgb(background.Color.R, background.Color.G, background.Color.B);
			}

			RoutedEventHandler selectAction = null;
			selectAction = (sender, args) =>
			{
				SelectEntry(selectPath);
				directoryView.Loaded -= selectAction;
			};
			directoryView.Loaded += selectAction;
		}

		private void SelectEntry(string path)
		{ 
			var beginItem = (BackgroundStorageDirectory)directoryView.Items[0];
			SelectEntry(path, beginItem, (TreeViewItem)directoryView.ItemContainerGenerator.ContainerFromItem(beginItem));
		}

		private void SelectEntry(string remainingSelectPath, BackgroundStorageDirectory currentNode, TreeViewItem currentContainer)
		{
			var i = remainingSelectPath.IndexOf('\\');
			if (i >= 0)
			{
				string next = remainingSelectPath.Substring(0, i);
				remainingSelectPath = remainingSelectPath.Substring(i + 1);

				currentNode = currentNode.Directories.First((dir) => dir.Name == next);
				currentContainer = currentContainer.ItemContainerGenerator.ContainerFromItem(currentNode) as TreeViewItem;
				currentContainer.IsExpanded = true;
				currentContainer.ItemContainerGenerator.StatusChanged += (sender, args) =>
				{
					if (currentContainer.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
						SelectEntry(remainingSelectPath, currentNode, currentContainer);
				};
			}
			else
			{
				imageListView.SelectedItem = currentNode.Files.Where(file => file.Name == remainingSelectPath).SingleOrDefault();
				currentContainer.IsSelected = true;
				currentContainer.BringIntoView();
				imageListView.Focus();
			}
		}

		private void SetResultAndClose()
		{
			if (UseImage)
			{
				var entry = (BackgroundStorageEntry)imageListView.SelectedItem;
				if (entry == null)
				{
					MessageBox.Show("Es ist kein Bild ausgewählt.");
					return;
				}

				ChosenBackground = new SongBackground(entry.Path.Substring(1).Replace('/', '\\'), entry.IsVideo);
			}
			else
			{
				ChosenBackground = new SongBackground(System.Drawing.Color.FromArgb(ColorPicker.SelectedColor.R, ColorPicker.SelectedColor.G, ColorPicker.SelectedColor.B));
			}

			this.DialogResult = true;
			this.Close();
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			SetResultAndClose();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				SetResultAndClose();
			}
		}

		private void directoryView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			ScrollViewer scrollViewer = imageListView.FindVisualChild<ScrollViewer>();
			scrollViewer.ScrollToTop();
		}

		private void image_ImageLoaderLoaded(object sender, RoutedEventArgs e)
		{
			var cp = (sender as DependencyObject).FindVisualParent<ContentPresenter>();
			if (cp.Content.Equals(imageListView.SelectedItem))
			{
				imageListView.ScrollIntoView(imageListView.SelectedItem);
			}
		}
	}
}
