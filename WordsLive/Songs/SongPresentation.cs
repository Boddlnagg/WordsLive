﻿/*
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

using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WordsLive.AudioVideo;
using WordsLive.Cef;
using WordsLive.Core;
using WordsLive.Core.Songs;
using WordsLive.Utils;

namespace WordsLive.Songs
{
	public class SongPresentation : CefPresentation
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
		private bool ignorePaint = false; // ignore paint events while in the middle of a reload (no transitions until done reloading)

		private int currentSlideIndex;
		public int CurrentSlideIndex
		{
			get
			{
				return currentSlideIndex;
			}
			set
			{
				DebugMessage($"Going to slide {value} (current is {currentSlideIndex})");
				ignorePaint = false;
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

		static void DebugMessage(string message)
		{
			//Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {message}");
		}

		public void Load(Song song, bool update = false)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			this.song = song;

			base.Load(false, true);

			DoubleAnimation ani = new DoubleAnimation { From = 0.0, To = 1.0 };
			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
			Storyboard.SetTarget(ani, Control.ForegroundGrid);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));

			if (song.VideoBackground == null)
			{
				frontImage = new ImageGrid { Background = Brushes.Black, Stretch = Stretch.UniformToFill };
				backImage = new ImageGrid { Background = Brushes.Black, Stretch = Stretch.UniformToFill };

				this.Control.BackgroundGrid.Children.Add(backImage);
				this.Control.ForegroundGrid.Children.Add(frontImage);
			}
			else
			{
				videoBackground = new AudioVideo.VlcWrapper(); // TODO: use configurable wrapper (does WPF work?)
				
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

			if (this.Control.Web.IsBrowserInitialized)
				Init();
			else
				(this.Control.Web as CefSharp.OffScreen.ChromiumWebBrowser).BrowserInitialized += SongPresentation_BrowserInitialized;
			
			currentSlideIndex = -1;

			if (update)
				ignorePaint = true;
		}

		private void Init()
		{
			// TODO: try to move to native JS transitions
			controller = new SongDisplayController(Control.Web, SongDisplayController.FeatureLevel.None);
			controller.ShowChords = showChords;

			controller.SongLoaded += (s, args) =>
			{
				OnFinishedLoading();
			};
			(Control.Web as CefSharp.OffScreen.ChromiumWebBrowser).Paint += SongPresentation_Paint;
			controller.Load(this.song);
		}

		private void SongPresentation_Paint(object sender, CefSharp.OffScreen.OnPaintEventArgs e)
		{
			if (ignorePaint)
			{
				DebugMessage($"Paint event for rect ({e.DirtyRect.X}/{e.DirtyRect.Y}, {e.DirtyRect.Width}/{e.DirtyRect.Height}) IGNORED");
				return;
			}
			DebugMessage($"Paint event for rect ({e.DirtyRect.X}/{e.DirtyRect.Y}, {e.DirtyRect.Width}/{e.DirtyRect.Height})");
			this.Control.Dispatcher.InvokeAsync(() =>
			{
				DebugMessage("Executing paint");
				UpdateSlide();
			});
		}

		private void SongPresentation_BrowserInitialized(object sender, EventArgs e)
		{
			(this.Control.Web as CefSharp.OffScreen.ChromiumWebBrowser).BrowserInitialized -= SongPresentation_BrowserInitialized;
			Init();
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
			{
				controller.GotoBlankSlide(bg);
			}

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
				DebugMessage($"Setting next background for slide {this.CurrentSlideIndex}");
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
				storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.SongSlideTransition);
				storyboard.Begin(this.Control.BackgroundGrid);
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
			(Control.Web as CefSharp.OffScreen.ChromiumWebBrowser).Paint -= SongPresentation_Paint;
			(this.Control.Web as CefSharp.OffScreen.ChromiumWebBrowser).BrowserInitialized -= SongPresentation_BrowserInitialized;

			base.Close();
			if (videoBackground != null)
				videoBackground.Destroy();
		}
	}
}
