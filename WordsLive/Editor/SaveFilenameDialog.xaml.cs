using System.ComponentModel;
using System.IO;
using System.Windows;
using WordsLive.Core.Data;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class SaveFilenameDialog : Window, IDataErrorInfo, INotifyPropertyChanged
	{
		string fileName;

		public SaveFilenameDialog(string name)
		{
			InitializeComponent();

			this.DataContext = this;
			this.FileNameWithoutExtension = name;
		}

		public string FileNameWithoutExtension
		{
			get
			{
				return fileName;
			}
			set
			{
				fileName = value;
				OnPropertyChanged("FileName");
			}
		}

		public string FileName
		{
			get
			{
				return fileName + ".ppl";
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

			bool notFound = false;
			try
			{
				var stream = DataManager.Songs.Get(FileName);
				stream.Close();
			}
			catch (FileNotFoundException)
			{
				notFound = true;
			}

			if (!notFound)
			{
				MessageBox.Show(WordsLive.Resources.Resource.sfFileExists);
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
					case "FileNameWithoutExtension":
						if (string.IsNullOrEmpty(this.fileName) || this.fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
							return WordsLive.Resources.Resource.sfInvalidFilename;

						break;
				}

				return null;
			}
		}
	}
}
