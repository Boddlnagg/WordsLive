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
using WordsLive.Resources;
using WinForms = System.Windows.Forms;

namespace WordsLive
{
	public enum ScreenIndex
	{
		Primary = 0,
		Secondary = 1
	}

	public class PresentationAreaSetting : INotifyPropertyChanged, ICloneable
	{
		public string Export()
		{
			return String.Format("{0},{1},{2},{3},{4},{5}", (int)ScreenIndex, Fullscreen.ToString(), Left, Top, Width, Height);
		}

		public static PresentationAreaSetting Import(string s)
		{ 
			var parts = s.Split(',');
			return new PresentationAreaSetting {
				ScreenIndex = (ScreenIndex)int.Parse(parts[0]),
				Fullscreen = bool.Parse(parts[1]),
				Left = int.Parse(parts[2]),
				Top = int.Parse(parts[3]),
				Width = int.Parse(parts[4]),
				Height = int.Parse(parts[5]),

			};
		}

		public string FullName
		{
			get
			{
				return String.Format("{0} ({1})", ScreenIndex == ScreenIndex.Primary ? Resource.paPrimary : Resource.paSecondary, Fullscreen ? Resource.paFullscreen : String.Format(Resource.paPositionSize, Left, Top, Width, Height));
			}
		}

		public void Update()
		{
			OnPropertyChanged("IsAvailable");
			OnPropertyChanged("FullName");
		}

		public bool IsAvailable
		{
			get
			{
				var screen = GetScreen();
				if (screen == null)
					return false;

				return Fullscreen || (screen.Bounds.Width >= Left + Width && screen.Bounds.Height >= Top + Height);
			}
		}

		private WinForms.Screen GetScreen()
		{
			int index = (int)ScreenIndex;
			if (WinForms.Screen.AllScreens.Length > index)
				return WinForms.Screen.AllScreens[index];
			else
				return null;
		}

		private ScreenIndex screenIndex;
		public ScreenIndex ScreenIndex
		{
			get
			{
				return screenIndex;
			}
			set
			{
				screenIndex = value;
				OnPropertyChanged("ScreenIndex");
				Update();
			}
		}

		private int left;
		public int Left
		{
			get
			{
				return left;
			}
			set
			{
				left = value;
				if (left < 0)
					left = 0;
				OnPropertyChanged("Left");
				Update();
			}
		}

		private int top;
		public int Top
		{
			get
			{
				return top;
			}
			set
			{
				top = value;
				if (top < 0)
					top = 0;
				OnPropertyChanged("Top");
				Update();
			}
		}

		private int width;
		public int Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
				if (width < 10)
					width = 10;
				OnPropertyChanged("Width");
				Update();
			}
		}

		private int height;
		public int Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
				if (height < 10)
					height = 10;
				OnPropertyChanged("Height");
				Update();
			}
		}

		private bool fullscreen;
		public bool Fullscreen
		{
			get
			{
				return fullscreen;
			}
			set
			{
				fullscreen = value;
				OnPropertyChanged("Fullscreen");
				OnPropertyChanged("IsNotFullscreen");
				Update();
			}
		}

		public bool IsNotFullscreen
		{
			get
			{
				return !fullscreen;
			}
		}

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public object Clone()
		{
			return new PresentationAreaSetting
			{
				Fullscreen = this.Fullscreen,
				ScreenIndex = this.ScreenIndex,
				Left = this.Left,
				Top = this.Top,
				Width = this.Width,
				Height = this.Height
			};
		}
	}
}
