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

			this.embeddedServerPasswordBox.Password = Properties.Settings.Default.EmbeddedServerPassword;
			this.dataServerPasswordBox.Password = Properties.Settings.Default.DataServerPassword;

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

		public bool UseDataServer
		{
			get
			{
				return Properties.Settings.Default.UseDataServer;
			}
			set
			{
				Properties.Settings.Default.UseDataServer = value;
			}
		}

		public bool UseLocalDirectories
		{
			get
			{
				return !UseDataServer;
			}
			set
			{
				UseDataServer = !value;
			}
		}

		public string DataServerAddress
		{
			get
			{
				return Properties.Settings.Default.DataServerAddress;
			}
			set
			{
				Properties.Settings.Default.DataServerAddress = value;
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

		#region Remoting

		public bool EmbeddedServerEnable
		{
			get
			{
				return Properties.Settings.Default.EmbeddedServerEnable;
			}
			set
			{
				Properties.Settings.Default.EmbeddedServerEnable = value;
			}
		}

		public int EmbeddedServerPort
		{
			get
			{
				return Properties.Settings.Default.EmbeddedServerPort;
			}
			set
			{
				Properties.Settings.Default.EmbeddedServerPort = value;
			}
		}

		public bool EmbeddedServerRedirectAll
		{
			get
			{
				return Properties.Settings.Default.EmbeddedServerRedirectAll;
			}
			set
			{
				Properties.Settings.Default.EmbeddedServerRedirectAll = value;
			}
		}

		public bool EmbeddedServerEnableUI
		{
			get
			{
				return Properties.Settings.Default.EmbeddedServerEnableUI;
			}
			set
			{
				Properties.Settings.Default.EmbeddedServerEnableUI = value;
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

		#endregion

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
					case "EmbeddedServerPort":
						if (EmbeddedServerPort != 80 && EmbeddedServerPort < 1024)
							return Resource.seErrorWrongPortNumber;
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
				Properties.Settings.Default.DataServerPassword = dataServerPasswordBox.Password;
				Properties.Settings.Default.EmbeddedServerPassword = embeddedServerPasswordBox.Password;
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
