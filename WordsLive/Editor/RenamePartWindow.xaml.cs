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

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Core.Songs;
using WordsLive.Resources;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class RenamePartWindow : Window, INotifyPropertyChanged, IDataErrorInfo
	{
		private Song song;
		private SongPart part;
		private string partName;

		public RenamePartWindow(Song song, SongPart part)
		{
			InitializeComponent();
			this.song = song;
			this.part = part;
			this.DataContext = this;
			if (this.part != null)
			{
				this.PartName = this.part.Name;
			}
		}
		
		public string PartName
		{
			get
			{
				return partName;
			}
			set
			{
				partName = value;
				OnNotifyPropertyChanged("PartName");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnNotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SetResult();
		}

		private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SetResult();
		}

		private void SetResult()
		{
			if (string.IsNullOrEmpty(partName))
			{
				this.newNameTextBox.Text = string.Empty;
			}

			if (!this.IsValid()) return;
			this.DialogResult = true;
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox box = (ListBox)sender;
			this.newNameTextBox.Text = (string)box.SelectedItem;
			this.newNameTextBox.Focus();
			int i = this.newNameTextBox.Text.IndexOf('#');
			if (i >= 0)
			{
				this.newNameTextBox.Select(i, 1);
			}
			else
			{
				this.newNameTextBox.Select(this.newNameTextBox.Text.Length, 0);
			}
		}

		public string Error
		{
			get { return null; }
		}

		public string this[string name]
		{
			get
			{
				switch (name)
				{ 
					case "PartName":
						if (string.IsNullOrEmpty(this.partName))
							return Resource.rpMsgNameMustNotBeEmpty;

						foreach (var part in this.song.Parts)
						{
							if (this.partName == part.Name && !(part == this.part && this.part != null))
								return Resource.rpMsgNameAlreadyExists;
						}
						break;
				}

				return null;
			}
		}
	}
}
