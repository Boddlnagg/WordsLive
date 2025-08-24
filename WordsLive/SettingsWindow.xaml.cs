/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Resources;
using WordsLive.Utils;

namespace WordsLive
{
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

		public int DocumentPageTransition
		{
			get
			{
				return Properties.Settings.Default.DocumentPageTransition;
			}
			set
			{
				Properties.Settings.Default.DocumentPageTransition = value;
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

		public bool UseVlc
		{
			get
			{
				return Properties.Settings.Default.UseVlc;
			}
			set
			{
				Properties.Settings.Default.UseVlc = value;
			}
		}

		public bool IsVlcAvailable
		{
			get
			{
				return AudioVideo.VlcController.IsAvailable;
			}
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
							return Resource.seErrorNegativeValue;
						break;
					case "SongSlideTransition":
						if (SongSlideTransition < 0)
							return Resource.seErrorNegativeValue;
						break;
					case "ImageTransition":
						if (ImageTransition < 0)
							return Resource.seErrorNegativeValue;
						break;
				}
				return null;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (this.IsValid())
			{
				this.DialogResult = true;
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
