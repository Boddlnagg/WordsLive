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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WordsLive.Presentation.Wpf;
using WordsLive.Utils.ImageLoader;

namespace WordsLive.Images
{
	public class ImagesPresentation : WpfPresentation<Grid>
	{
		Grid backGrid, frontGrid;
		Image back, front;

		Storyboard storyboard;
		bool animationRunning;
		ImageInfo current;
		ImageInfo next;

		public bool IsLoadingImage { get; private set; }

		public ImagesPresentation()
		{
			back = new Image();
			backGrid = new Grid { Background = Brushes.Black };
			backGrid.Children.Add(back);
			front = new Image();
			frontGrid = new Grid { Background = Brushes.Black };
			frontGrid.Children.Add(front);

			this.Control.Children.Add(backGrid);
			this.Control.Children.Add(frontGrid);

			Loader.SetDisplayOption(back, DisplayOptions.FullResolution);
			Loader.SetMaxSize(back, Math.Max(Area.Width, Area.Height));
			Loader.AddLoadedHandler(back, back_Loaded);
			this.Area.WindowSizeChanged	+= Area_WindowSizeChanged;

			DoubleAnimation ani = new DoubleAnimation { From = 1.0, To = 0.0, FillBehavior = FillBehavior.Stop };
			ani.Completed += ani_Completed;
			Storyboard.SetTarget(ani, frontGrid);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));

			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
		}

		private void Area_WindowSizeChanged(object sender, EventArgs e)
		{
			Loader.SetMaxSize(back, Math.Max(Area.Width, Area.Height));
		}

		void ani_Completed(object sender, EventArgs e)
		{
			front.Source = back.Source;
			frontGrid.Opacity = 1;
			animationRunning = false;
			if (next != null)
			{
				Update(next);
				next = null;
			}
			else
			{
				OnLoadingFinished();
			}
		}

		public ImageInfo CurrentImage
		{
			get
			{
				return current;
			}
			set
			{
				current = value;

				if (animationRunning)
				{
					next = value;
				}
				else
				{
					Update(value);
				}
			}
		}

		public event EventHandler LoadingFinished;

		protected virtual void OnLoadingFinished()
		{
			if (LoadingFinished != null)
				LoadingFinished(this, EventArgs.Empty);
		}

		private void back_Loaded(object sender, RoutedEventArgs e)
		{
			animationRunning = true;
			storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.ImageTransition);
			storyboard.Begin(this.Control);
			IsLoadingImage = false;
		}

		private void Update(ImageInfo image)
		{
			IsLoadingImage = true;
			Loader.SetSourceType(back, image.SourceType);
			Loader.SetSource(back, image.Source);
		}

		public override void Close()
		{
			this.Area.WindowSizeChanged -= Area_WindowSizeChanged;
			base.Close();
		}
	}
}
