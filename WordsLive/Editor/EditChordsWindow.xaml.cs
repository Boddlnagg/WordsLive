using System.ComponentModel;
using System.Windows;
using Words.Core.Songs.Chords;
using System;

namespace Words.Editor
{
	public partial class EditChordsWindow : Window, INotifyPropertyChanged, IDataErrorInfo
	{
		private Key originalKey = null;
		private int transposeAmount;
		private bool germanNotation;
		private bool longChordNames;

		private SongNodeRoot song;

		public EditChordsWindow(SongNodeRoot song)
		{
			InitializeComponent();

			this.song = song;

			DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		public string OriginalKey
		{
			get
			{
				return originalKey == null ? String.Empty : originalKey.ToString();
			}
			set
			{
				try
				{
					originalKey = new Key(value);
				}
				catch (ArgumentException)
				{
					originalKey = null;
				}
				OnNotifyPropertyChanged("OriginalKey");
				OnNotifyPropertyChanged("TargetKey");
			}
		}

		public int TransposeAmount
		{
			get
			{
				return transposeAmount;
			}
			set
			{
				transposeAmount = Note.NormalizeSemitones(value);
				OnNotifyPropertyChanged("TransposeAmount");
				OnNotifyPropertyChanged("TargetKey");
			}
		}

		public string TargetKey
		{
			get
			{
				return originalKey == null ? String.Empty : originalKey.Transpose(TransposeAmount).ToString();
			}
		}

		public bool GermanNotation
		{
			get
			{
				return germanNotation;
			}
			set
			{
				germanNotation = value;
				Chords.GermanNotation = germanNotation;
				OnNotifyPropertyChanged("GermanNotation");
			}
		}

		public bool LongChordNames
		{
			get
			{
				return longChordNames;
			}
			set
			{
				longChordNames = value;
				Chords.LongChordNames = longChordNames;
				OnNotifyPropertyChanged("LongChordNames");
			}
		}

		private void ButtonTranspose_Click(object sender, RoutedEventArgs e)
		{
			song.TransposeChords(originalKey, TransposeAmount);
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
					case "OriginalKey":
						if (this.originalKey == null)
							return "Geben Sie eine gültige Tonart ein.";
						break;
				}

				return null;
			}
		}

		private void ButtonRemoveChords_Click(object sender, RoutedEventArgs e)
		{
			song.RemoveAllChords();
		}
	}
}
