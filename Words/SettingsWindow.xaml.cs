﻿using System.ComponentModel;
using System.Windows;
using Words.Utils;

namespace Words
{
	/// <summary>
	/// Interaktionslogik für SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window, IDataErrorInfo
	{
		public SettingsWindow()
		{
			InitializeComponent();
			this.DataContext = this;
		}

		public string SongsDirectory
		{
			get
			{
				return Properties.Settings.Default.SongsDirectory;
			}
			set
			{
				Properties.Settings.Default.SongsDirectory = value;
			}
		}

		public string BackgroundsDirectory
		{
			get
			{
				return Properties.Settings.Default.BackgroundsDirectory;
			}
			set
			{
				Properties.Settings.Default.BackgroundsDirectory = value;
			}
		}

		public int PresentationTransition
		{
			get
			{
				return Properties.Settings.Default.PresentationTransition;
			}
			set
			{
				Properties.Settings.Default.PresentationTransition = value;
			}
		}

		public int SongSlideTransition
		{
			get
			{
				return Properties.Settings.Default.SongSlideTransition;
			}
			set
			{
				Properties.Settings.Default.SongSlideTransition = value;
			}
		}

		public string SongTemplateFile
		{
			get
			{
				return Properties.Settings.Default.SongTemplateFile;
			}
			set
			{
				Properties.Settings.Default.SongTemplateFile = value;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (this.IsValid())
				this.Close();
		}

		public string Error
		{
			get { return null; }
		}

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "PresentationTransition":
						if (PresentationTransition < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "SongSlideTransition":
						if (SongSlideTransition < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
				}
				return null;
			}
		}
	}
}
