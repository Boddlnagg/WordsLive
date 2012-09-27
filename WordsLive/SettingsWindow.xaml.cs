using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Utils;

namespace WordsLive
{
	/// <summary>
	/// Interaktionslogik für SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window, IDataErrorInfo
	{
		private static List<Type> tabs = new List<Type>();

		internal static List<Type> Tabs
		{
			get
			{
				return tabs;
			}
		}

		public SettingsWindow()
		{
			InitializeComponent();

			foreach(var tabType in tabs)
			{
				var tab = (ISettingsTab)Activator.CreateInstance(tabType);
				this.TabControl.Items.Add(new TabItem { Header = tab.Header, Content = tab.Control });
			}

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

		public int ImageTransition
		{
			get
			{
				return Properties.Settings.Default.ImageTransition;
			}
			set
			{
				Properties.Settings.Default.ImageTransition = value;
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
					// TODO: localize
					case "PresentationTransition":
						if (PresentationTransition < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "SongSlideTransition":
						if (SongSlideTransition < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
					case "ImageTransition":
						if (ImageTransition < 0)
							return "Der Wert muss 0 oder größer sein.";
						break;
				}
				return null;
			}
		}
	}
}
