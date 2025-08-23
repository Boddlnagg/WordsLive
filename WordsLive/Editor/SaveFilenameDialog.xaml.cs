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
using WordsLive.Core;
using WordsLive.Resources;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class SaveFilenameDialog : Window, IDataErrorInfo, INotifyPropertyChanged
	{
		string filename;

		public SaveFilenameDialog(string name)
		{
			InitializeComponent();

			this.DataContext = this;
			this.FilenameWithoutExtension = name;

			this.Loaded += delegate
			{
				this.filenameTextBox.Focus();
				this.filenameTextBox.SelectAll();
			};
		}

		public string FilenameWithoutExtension
		{
			get
			{
				return filename;
			}
			set
			{
				filename = value;
				OnPropertyChanged("Filename");
			}
		}

		public string Filename
		{
			get
			{
				return filename + ".ppl";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SetResult();
		}

		private void SetResult()
		{
			if (!this.IsValid()) return;

			if (DataManager.Songs.Exists(Filename))
			{
				var overwrite = MessageBox.Show(this, Resource.sfMsgOverwriteExistingFile, Resource.sfMsgOverwriteExistingFileTitle, MessageBoxButton.YesNoCancel);
				if (overwrite != MessageBoxResult.Yes)
					return;
			}

			this.DialogResult = true;
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
					case "FilenameWithoutExtension":
						if (!DataManager.Songs.IsValidName(this.filename))
							return Resource.sfInvalidFilename;

						break;
				}

				return null;
			}
		}
	}
}
