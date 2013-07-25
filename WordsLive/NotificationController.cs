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
using System.Windows;
using WordsLive.Presentation.Wpf;

namespace WordsLive
{
	public class NotificationController : INotifyPropertyChanged
	{
		private static NotificationController instance;

		public static NotificationController Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new NotificationController();
				}

				return instance;
			}
		}

		private string text;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (text != value)
				{
					text = value;
					OnPropertyChanged("Text");
					OnPropertyChanged("TextAvailable");
				}
			}
		}

		public bool TextAvailable
		{
			get
			{
				return !String.IsNullOrEmpty(Text);
			}
		}

		private VerticalAlignment alignment;

		public VerticalAlignment Alignment
		{
			get
			{
				return alignment;
			}
			set
			{
				alignment = value;
				OnPropertyChanged("Alignment");
			}
		}

		private bool isShown;

		public bool IsShown
		{
			get
			{
				return isShown;
			}
			private set
			{
				if (isShown != value)
				{
					isShown = value;
					OnPropertyChanged("IsShown");
				}
			}
		}

		public void Show()
		{
			WpfPresentationWindow.ShowNotification(Text, Alignment);
			IsShown = true;
		}

		public void Hide()
		{
			WpfPresentationWindow.HideNotification();
			IsShown = false;
		}

		protected NotificationController()
		{
			Text = "";
			Alignment = VerticalAlignment.Top;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private NotificationSettingsWindow window;

		public void ShowSettingsWindow()
		{
			if (window == null)
			{
				window = new NotificationSettingsWindow();
				window.Closed += window_Closed;
				Controller.ShowWindow(window);
			}
			else
			{
				window.Focus();
			}
		}

		void window_Closed(object sender, EventArgs e)
		{
			window.Closed -= window_Closed;
			window = null;
		}
	}
}
