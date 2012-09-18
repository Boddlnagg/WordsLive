using System.ComponentModel;
using System.Windows;
using WordsLive.Utils;

namespace WordsLive.Editor
{
    /// <summary>
    /// Interaktionslogik für RenameSongWindow.xaml
    /// </summary>
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
                            return WordsLive.Resources.Resource.rsMsgNameMustNotBeEmpty;
                        break;
                }
                return null;
            }
        }
    }
}
