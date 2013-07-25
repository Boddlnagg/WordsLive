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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WordsLive.AudioVideo
{
	[TargetMedia(typeof(AudioVideoMedia))]
	public partial class AudioVideoControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private AudioVideoMedia media;
		private IAudioVideoPresentation presentation;
		private PlayState playState = PlayState.Stopped;
		private bool disableSeek = false;
		private ControlPanelLoadState loadState = ControlPanelLoadState.Loading;

		public AudioVideoControlPanel()
		{
			InitializeComponent();

			this.DataContext = this;

			//Controller.PresentationManager.Area.WindowSizeChanged += (sender, args) =>
			//{
			//	this.preview.Width = Controller.PresentationManager.Area.WindowSize.Width;
			//	this.preview.Height = Controller.PresentationManager.Area.WindowSize.Height;
			//};
			//this.preview.Width = Controller.PresentationManager.Area.WindowSize.Width;
			//this.preview.Height = Controller.PresentationManager.Area.WindowSize.Height;
		}

		public PlayState PlayState
		{
			get
			{
				return playState;
			}
			private set
			{
				if (playState != value)
				{
					playState = value;
					OnPropertyChanged("PlayState");
				}
			}
		}

		public bool IsLooping
		{
			get
			{
				if (presentation == null)
					return false;
				
				return presentation.MediaControl.Loop;
			}
			set
			{
				if (presentation != null)
				{
					presentation.MediaControl.Loop = value;
					OnPropertyChanged("IsLooping");
				}
			}
		}

		public Control Control
		{
			get { return this; }
		}

		public Core.Media Media
		{
			get { return media; }
		}

		public ControlPanelLoadState LoadState
		{
			get
			{
				return loadState;
			}
			set
			{
				loadState = value;
				OnPropertyChanged("LoadState");
			}
		}

		public void Init(Core.Media media)
		{
			this.media = media as AudioVideoMedia;

			if (this.media == null)
				throw new ArgumentException("media must not be null and of type VideoMedia");

			if (Properties.Settings.Default.UseVlc && VlcController.IsAvailable)
			{
				var vlc = Controller.PresentationManager.CreatePresentation<AudioVideoPresentation<VlcWrapper>>();
				Load(vlc);
			}
			else
			{
				var wpf = Controller.PresentationManager.CreatePresentation<AudioVideoPresentation<WpfWrapper>>();
				Load(wpf);
			}
		}

		private static string FormatTimeSpan(TimeSpan span)
		{
			return String.Format("{0:00}:{1:00}", (int)span.TotalMinutes, span.Seconds);
		}

		private void SetOffsets(TimeSpan start, TimeSpan end)
		{
			media.OffsetStart = start;
			media.OffsetEnd = end;
			timelineSlider.SelectionStart = media.OffsetStart.TotalMilliseconds;
			timelineSlider.SelectionEnd = presentation.MediaControl.Duration.TotalMilliseconds - media.OffsetEnd.TotalMilliseconds;
		}

		private void Load<T>(AudioVideoPresentation<T> pres) where T : BaseMediaControl,  new()
		{
			presentation = pres;
			presentation.MediaControl.MediaLoaded += () =>
			{
				timelineSlider.Maximum = presentation.MediaControl.Duration.TotalMilliseconds;
				timelineSlider.IsSelectionRangeEnabled = true;
				SetOffsets(media.OffsetStart, media.OffsetEnd);
				timelineSlider.Value = media.OffsetStart.TotalMilliseconds;
				volumeSlider.Value = presentation.MediaControl.Volume;
				totalTimeLabel.Content = FormatTimeSpan(presentation.MediaControl.Duration);
				// TODO: Show presentation only when started (-> no preview would be available)?
				Controller.PresentationManager.CurrentPresentation = presentation;
				//preview.Fill = new System.Windows.Media.VisualBrush(presentation.MediaControl);
				LoadState = ControlPanelLoadState.Loaded;
			};

			presentation.MediaControl.MediaFailed += () =>
			{
				LoadState = ControlPanelLoadState.Failed;
			};

			presentation.MediaControl.PlaybackEnded += () =>
			{
				PlayState = PlayState.Stopped;
			};

			presentation.MediaControl.SeekStart += () =>
			{
				timelineSlider.Value = timelineSlider.SelectionStart;
				presentation.MediaControl.Position = (int)timelineSlider.Value;
			};

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
			timer.Tick += (sender, args) =>
			{
				if (LoadState == ControlPanelLoadState.Loaded)
				{
					disableSeek = true;
					timelineSlider.Value = presentation.MediaControl.Position;
					disableSeek = false;

					// if there is an end-offset, stop before end of playback
					if (PlayState == AudioVideo.PlayState.Playing && media.OffsetEnd.TotalMilliseconds > 0 && timelineSlider.Value >= timelineSlider.SelectionEnd)
					{
						presentation.MediaControl.Stop();
						if (IsLooping)
						{
							// TODO: this sometimes doesn't work with VLC (starts from beginning instead of start-offset)
							presentation.MediaControl.Play();
						}
						else
						{
							PlayState = AudioVideo.PlayState.Stopped;
						}
					}
					
					currentTimeLabel.Content = FormatTimeSpan(new TimeSpan(0, 0, 0, 0, presentation.MediaControl.Position));
				}
			};
			timer.Start();
			
			presentation.MediaControl.Load(media.Uri);
		}

		public bool IsUpdatable
		{
			get { return false; }
		}

		private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!disableSeek)
				presentation.MediaControl.Position = (int)timelineSlider.Value;
		}

		private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (presentation != null)
				presentation.MediaControl.Volume = (int)volumeSlider.Value;
		}

		private void OnMouseDownStopMedia(object sender, RoutedEventArgs e)
		{
			Stop();
		}

		private void Stop()
		{
			presentation.MediaControl.Stop();
			PlayState = PlayState.Stopped;
		}

		private void OnMouseDownPlayPauseMedia(object sender, RoutedEventArgs e)
		{
			TogglePlayPause();
		}

		public void Close()
		{
			if (presentation != null && Controller.PresentationManager.CurrentPresentation == presentation)
				presentation.Close(); // TODO: really close (stop) the presentation when we're closing the control panel?
		}

		private void OnMouseDownSetOffset(object sender, RoutedEventArgs e)
		{
			switch ((sender as Button).Tag as string)
			{
				case "Start":
					SetOffsets(new TimeSpan(0, 0, 0, 0, (int)timelineSlider.Value), media.OffsetEnd);
					break;
				case "End":
					SetOffsets(media.OffsetStart, new TimeSpan(0, 0, 0, 0, (int)(timelineSlider.Maximum - timelineSlider.Value)));
					break;
			}
		}

		private void OnMouseDownResetOffset(object sender, RoutedEventArgs e)
		{
			switch ((sender as Button).Tag as string)
			{
				case "Start":
					SetOffsets(new TimeSpan(0), media.OffsetEnd);
					break;
				case "End":
					SetOffsets(media.OffsetStart, new TimeSpan(0));
					break;
			}
		}

		private void TogglePlayPause()
		{
			if (PlayState == PlayState.Stopped || PlayState == PlayState.Paused)
			{
				presentation.MediaControl.Play();
				PlayState = PlayState.Playing;
			}
			else
			{
				presentation.MediaControl.Pause();
				PlayState = PlayState.Paused;
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				TogglePlayPause();
			}

			e.Handled = true;

			base.OnPreviewKeyDown(e);
		}

		private void Control_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(playPauseButton);
		}
	}
}
