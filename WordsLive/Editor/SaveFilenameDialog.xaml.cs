using System.ComponentModel;
using System.IO;
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
						if (string.IsNullOrEmpty(this.filename) || this.filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
							return Resource.sfInvalidFilename;

						break;
				}

				return null;
			}
		}
	}
}
