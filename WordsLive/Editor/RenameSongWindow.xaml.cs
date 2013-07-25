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
using WordsLive.Resources;
using WordsLive.Utils;

namespace WordsLive.Editor
{
    public partial class RenameSongWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        private string songName;
        public string SongName
        {
            get
            {
                return songName;
            }
            set
            {
                songName = value;
                OnNotifyPropertyChanged("SongName");
            }
        }

        public RenameSongWindow(string songName)
        {
            InitializeComponent();
            this.SongName = songName;
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsValid()) return;
            this.DialogResult = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnNotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
                    case "SongName":
                        if (string.IsNullOrEmpty(this.songName))
                            return Resource.rsMsgNameMustNotBeEmpty;
                        break;
                }
                return null;
            }
        }
    }
}
