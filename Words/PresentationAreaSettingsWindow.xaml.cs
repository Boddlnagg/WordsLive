using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using WinForms = System.Windows.Forms;
using Words.Resources;

namespace Words
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
		}

		private void updateButton_Click(object sender, RoutedEventArgs e)
		{
			OnPropertyChanged("IsSecondaryScreenAvailable"); // TODO (Words): testen, wenn ein zweiter Bildschirm zur Verfügung steht

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
