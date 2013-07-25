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
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using WordsLive.AudioVideo;
using WordsLive.Awesomium;
using WordsLive.Core;
using WordsLive.Core.Songs;
using WordsLive.Utils;

namespace WordsLive.Songs
{
	public class SongPresentation : AwesomiumPresentation
	{
		private Song song;
		private Dictionary<SongSlide, int> slides = new Dictionary<SongSlide, int>();
		private SongDisplayController controller;
		private ImageGrid frontImage;
		private ImageGrid backImage;
		private Storyboard storyboard;
		private BaseMediaControl videoBackground;
		private System.Windows.Shapes.Rectangle videoBackgroundClone;
		private ImageSource nextBackground = null;

		private int currentSlideIndex;
		public int CurrentSlideIndex
		{
			get
			{
				return currentSlideIndex;
			}
			set
			{
				if (currentSlideIndex != value)
				{
					GotoSlide(value);
					currentSlideIndex = value;
				}
			}
		}

		private bool showChords;

		public bool ShowChords
		{
			get
			{
				return showChords;
			}
			set
			{
				showChords = value;
				if (controller != null)
				{
					controller.ShowChords = showChords;
				}
			}
		}

		private bool disableNextSlideTransition = false;

		public void Load(Song song, bool update = false)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			this.song = song;

			base.Load(false, true);

			if (update)
				disableNextSlideTransition = true;

			DoubleAnimation ani = new DoubleAnimation { From = 0.0, To = 1.0 };
			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
			Storyboard.SetTarget(ani, Control.ForegroundGrid);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));

			if (song.VideoBackground == null)
			{
				frontImage = new ImageGrid { Background = Brushes.Black, Stretch = Stretch.Fill };
				backImage = new ImageGrid { Background = Brushes.Black, Stretch = Stretch.Fill };

				this.Control.BackgroundGrid.Children.Add(backImage);
				this.Control.ForegroundGrid.Children.Add(frontImage);
			}
			else
			{
				videoBackground = new AudioVideo.VlcWrapper(); // TODO: use configurable wrapper
				
				videoBackground.Autoplay = true;
				videoBackground.Loop = true;
				try
				{
					videoBackground.Load(DataManager.Backgrounds.GetFile(song.VideoBackground).Uri);
				}
				catch (FileNotFoundException)
				{
					throw new NotImplementedException("Video background file not found: Doesn't know what to do.");
				}

				var brush = new System.Windows.Media.VisualBrush(videoBackground);
				videoBackgroundClone = new System.Windows.Shapes.Rectangle();
				videoBackgroundClone.Fill = brush;

				this.Control.ForegroundGrid.Children.Add(videoBackground);
				this.Control.BackgroundGrid.Children.Add(videoBackgroundClone);
			}

			this.Control.Web.IsTransparent = true;
			this.Control.Web.ProcessCreated += Web_ProcessCreated;
			currentSlideIndex = -1;
		}

		void Web_ProcessCreated(object sender, WebViewEventArgs e)
		{
			controller = new SongDisplayController(Control.Web, SongDisplayController.FeatureLevel.None);
			controller.ShowChords = showChords;

			controller.SongLoaded += (s, args) =>
			{
				OnFinishedLoading();
			};

			(Control.Web.Surface as ImageSurface).Updated += web_Updated;

			controller.Load(this.song);
		}

		void web_Updated(object sender, SurfaceUpdatedEventArgs e)
		{
			UpdateSlide();
		}

		public event EventHandler FinishedLoading;

		protected void OnFinishedLoading()
		{
			if (FinishedLoading != null)
				FinishedLoading(this, EventArgs.Empty);
		}

		private void GotoSlide(int index)
		{
			var tuple = FindSlideByIndex(index);

			SongBackground bg;

			if (tuple != null)
				bg = song.Backgrounds[tuple.Item1.BackgroundIndex];
			else if (index == 0) // first (blank) slide
				bg = song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0];
			else // last (blank) slide
				bg = song.Backgrounds[song.LastSlide != null ? song.LastSlide.BackgroundIndex : 0];

			if (tuple != null)
				controller.GotoSlide(tuple.Item2, tuple.Item3);
			else
				controller.GotoBlankSlide(bg);

			if (videoBackground == null) // only change backgrounds if we're not using a video background
			{
				nextBackground = SongBackgroundToImageSourceConverter.CreateBackgroundSource(bg);
			}
		}

		private void UpdateSlide()
		{
			Control.UpdateForeground();
			if (nextBackground != null)
			{
				backImage.Source = frontImage.Source;
				frontImage.Source = nextBackground;
				nextBackground = null;
			}
			else if (videoBackground == null)
			{
				backImage.Source = frontImage.Source;
			}

			if (videoBackground != null || backImage.Source != null)
			{
				storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, disableNextSlideTransition ? 0 : Properties.Settings.Default.SongSlideTransition);
				storyboard.Begin(this.Control.BackgroundGrid);
				disableNextSlideTransition = false;
			}
		}

		private Tuple<SongSlide, SongPartReference, int> FindSlideByIndex(int index)
		{
			int i = 1;
			SongPartReference pref = null;
			SongSlide slide = null;
			foreach (var partRef in song.Order)
			{
				pref = partRef;
				SongPart part = partRef.Part;
				int iInPart = 0;
				foreach (var s in part.Slides)
				{
					if (i++ == index)
					{
						slide = s;
						return new Tuple<SongSlide, SongPartReference, int>(slide, pref, iInPart);
					}
					iInPart++;
				}
			}

			return null;
		}

		public override void Close()
		{
			(Control.Web.Surface as ImageSurface).Updated -= web_Updated;
			Control.Web.ProcessCreated -= Web_ProcessCreated;

			base.Close();
			if (videoBackground != null)
				videoBackground.Destroy();
		}
	}
}
