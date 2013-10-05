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
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using Ionic.Zip;
using WordsLive.Resources;

namespace WordsLive
{
	public partial class FirstStartWindow : Window, INotifyPropertyChanged
	{
		public bool IsPowerpraiseInstalled
		{
			get;
			private set;
		}

		public string DisplayDirectory
		{
			get
			{
				if (usePowerpraiseData)
				{
					return PowerpraiseDirectory;
				}
				else
				{
					return SelectedDirectory;
				}
			}
			set
			{
				if (!usePowerpraiseData)
				{
					SelectedDirectory = value;
				}
			}
		}

		public string PowerpraiseDirectory
		{
			get;
			private set;
		}

		private string selectedDirectory;

		public string SelectedDirectory
		{
			get
			{
				return selectedDirectory;
			}
			set
			{
				selectedDirectory = value;
				OnPropertyChanged("SelectedDirectory");
				OnPropertyChanged("DisplayDirectory");
			}
		}

		private bool usePowerpraiseData;

		public bool UsePowerpraiseData
		{
			get
			{
				return usePowerpraiseData;
			}
			set
			{
				usePowerpraiseData = value;
				OnPropertyChanged("UsePowerpraiseData");
				OnPropertyChanged("UseSelectedDirectory");
				OnPropertyChanged("DisplayDirectory");
			}
		}

		public bool UseCustomDirectory
		{
			get
			{
				return !usePowerpraiseData;
			}
		}

		public FirstStartWindow()
		{
			// check for Powerpraise installation
			try
			{
				var powerpraiseSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Powerpraise", "Settings30.dat");
				if (File.Exists(powerpraiseSettings))
				{
					XDocument doc = XDocument.Load(powerpraiseSettings);
					PowerpraiseDirectory = doc.Element("settings").Element("startup").Element("userfilespath").Value;
					if (Directory.Exists(PowerpraiseDirectory))
					{
						IsPowerpraiseInstalled = true;
					}
				}
			}
			catch { }

			SelectedDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WordsLive");

			this.DataContext = this;

			InitializeComponent();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			var actualDirectory = DisplayDirectory;

			Properties.Settings.Default.SongsDirectory = Path.Combine(actualDirectory, "Songs");
			Properties.Settings.Default.BackgroundsDirectory = Path.Combine(actualDirectory, "Backgrounds");

			try
			{
				if (!UsePowerpraiseData)
				{
					Directory.CreateDirectory(Properties.Settings.Default.SongsDirectory);
					Directory.CreateDirectory(Properties.Settings.Default.BackgroundsDirectory);

					var appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
					var examplesZip = ZipFile.Read(Path.Combine(appDir.FullName, "Data", "Examples.zip"));
					examplesZip.ExtractAll(actualDirectory);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format(Resource.fsCouldNotInitDirectory, ex.Message));
				e.Cancel = true;
			}
		}

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
