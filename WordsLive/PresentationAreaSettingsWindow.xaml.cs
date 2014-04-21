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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WordsLive.Resources;
using WinForms = System.Windows.Forms;

namespace WordsLive
{
	public partial class PresentationAreaSettingsWindow : Window, INotifyPropertyChanged
	{
		private ObservableCollection<PresentationAreaSetting> settings = new ObservableCollection<PresentationAreaSetting>();

		public IEnumerable<PresentationAreaSetting> Settings
		{
			get
			{
				foreach (var setting in settings)
				{
					yield return setting;
				}
			}
			set
			{
				foreach (var setting in value)
				{
					this.settings.Add((PresentationAreaSetting)setting.Clone());
				}
			}
		}

		public PresentationAreaSettingsWindow()
		{
			InitializeComponent();
			
			settingsPriorityListBox.DataContext = this.settings;

			Controller.DisplaySettingsChanged += new EventHandler(Controller_DisplaySettingsChanged);
		}

		void Controller_DisplaySettingsChanged(object sender, EventArgs e)
		{
			Update();
		}

		private void updateButton_Click(object sender, RoutedEventArgs e)
		{
			Update();
		}

		private void Update()
		{
			OnPropertyChanged("IsSecondaryScreenAvailable");

			foreach (var setting in settings)
				setting.Update();
		}

		public bool IsSecondaryScreenAvailable
		{
			get
			{
				return WinForms.Screen.AllScreens.Length > (int)ScreenIndex.Secondary;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void moveUpButton_Click(object sender, RoutedEventArgs e)
		{
			if (settingsPriorityListBox.SelectedIndex > 0)
			{ 
				int index = settingsPriorityListBox.SelectedIndex;
				var item = (PresentationAreaSetting)settingsPriorityListBox.SelectedItem;
				settings.RemoveAt(index);
				settings.Insert(index - 1, item);
				settingsPriorityListBox.SelectedItem = item;
			}
		}

		private void moveDownButton_Click(object sender, RoutedEventArgs e)
		{
			if (settingsPriorityListBox.SelectedIndex < 0 || settingsPriorityListBox.SelectedIndex >= settings.Count - 1)
				return;

			int index = settingsPriorityListBox.SelectedIndex;
			var item = (PresentationAreaSetting)settingsPriorityListBox.SelectedItem;
			settings.RemoveAt(index);
			settings.Insert(index + 1, item);
			settingsPriorityListBox.SelectedItem = item;
		}

		private void addButton_Click(object sender, RoutedEventArgs e)
		{
			settings.Add(new PresentationAreaSetting { ScreenIndex = ScreenIndex.Secondary, Fullscreen = true });
		}

		private void removeButton_Click(object sender, RoutedEventArgs e)
		{
			settings.Remove((PresentationAreaSetting)settingsPriorityListBox.SelectedItem);
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			if (settings.Count(s => s.IsAvailable) == 0)
			{
				MessageBox.Show(Resource.paMsgValidOption);
			}
			else
			{
				DialogResult = true;
				this.Close();
			}
		}
	}
}
