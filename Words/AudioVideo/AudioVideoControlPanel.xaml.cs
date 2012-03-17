using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Presentation.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Words.AudioVideo
{
	[TargetMedia(typeof(AudioVideoMedia))]
	public partial class AudioVideoControlPanel : UserControl, IMediaControlPanel
	{
		private AudioVideoMedia media;
		private IAudioVideoPresentation presentation;
		private bool isPlaying = false;
		private bool loaded = false;
		private bool disableSeek = false;

		public AudioVideoControlPanel()
		{
			InitializeComponent();
		}

		public Control Control
		{
			get { return this; }
		}

		public Core.Media Media
		{
			get { return media; }
		}

		public void Init(Core.Media media)
		{
			this.media = media as AudioVideoMedia;

			if (this.media == null)
				throw new ArgumentException("media must not be null and of type VideoMedia");
		}

		private static string FormatTimeSpan(TimeSpan span)
		{
			return String.Format("{0:00}:{1:00}", (int)span.TotalMinutes, span.Seconds);
		}

		private void Load<T>(AudioVideoPresentation<T> pres) where T : BaseMediaControl,  new()
		{
			presentation = pres;
			presentation.MediaControl.MediaLoaded += () =>
			{
				timelineSlider.Maximum = presentation.MediaControl.Duration.TotalMilliseconds;
				volumeSlider.Value = presentation.MediaControl.Volume;
				totalTimeLabel.Content = FormatTimeSpan(presentation.MediaControl.Duration);
				loaded = true;
			};

			presentation.MediaControl.PlaybackEnded += () =>
			{
				playPauseButton.Content = "Play";
				isPlaying = false;
			};

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
			timer.Tick += (sender, args) =>
			{
				if (loaded)
				{
					disableSeek = true;
					timelineSlider.Value = presentation.MediaControl.Position;
					disableSeek = false;
					currentTimeLabel.Content = FormatTimeSpan(new TimeSpan(0, 0, 0, 0, presentation.MediaControl.Position));
				}
			};
			timer.Start();
			Controller.PresentationManager.CurrentPresentation = presentation;
			if (autoplayCheckBox.IsChecked.HasValue && autoplayCheckBox.IsChecked.Value)
			{
				presentation.MediaControl.Autoplay = true;
				isPlaying = true;
				playPauseButton.Content = "Pause";
			}
			presentation.MediaControl.Load(media.File);
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

		private void OnMouseDownPauseMedia(object sender, RoutedEventArgs e)
		{
			presentation.MediaControl.Pause();
		}

		private void OnMouseDownStopMedia(object sender, RoutedEventArgs e)
		{
			Stop();
		}

		private void Stop()
		{
			presentation.MediaControl.Stop();
			isPlaying = false;
			playPauseButton.Content = "Play";
		}

		private void OnMouseDownPlayPauseMedia(object sender, RoutedEventArgs e)
		{
			if (!isPlaying)
			{
				presentation.MediaControl.Play();
				playPauseButton.Content = "Pause";
				isPlaying = true;
			}
			else
			{
				presentation.MediaControl.Pause();
				playPauseButton.Content = "Play";
				isPlaying = false;
			}
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e)
		{
			switch((string)((sender as Button).Tag))
			{
				case "WPF":
					var wpf = Controller.PresentationManager.CreatePresentation<AudioVideoPresentation<WpfWrapper>>();
					Load(wpf);
					break;
				case "VLC":
					if (VlcController.IsAvailable)
					{
						var vlc = Controller.PresentationManager.CreatePresentation<AudioVideoPresentation<VlcWrapper>>();
						Load(vlc);
					}
					else
					{
						MessageBox.Show("VLC is not available on this system. Using WPF instead.");
						goto case "WPF";
					}
					break;
			}

			loadPanel.IsEnabled = false;
			controlPanel.IsEnabled = true;
		}

		private void loopCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			presentation.MediaControl.Loop = loopCheckBox.IsChecked.HasValue && loopCheckBox.IsChecked.Value;
		} 

		public void Close()
		{
			// We're setting the presentation to be the current presentation as soon as we create it,
			// so actually there's no need to do anything here ... check anyway
			if (Controller.PresentationManager.CurrentPresentation == presentation)
				presentation.Close();
		}
	}
}
