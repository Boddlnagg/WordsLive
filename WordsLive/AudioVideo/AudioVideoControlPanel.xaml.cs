using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

		private void Load<T>(AudioVideoPresentation<T> pres) where T : BaseMediaControl,  new()
		{
			presentation = pres;
			presentation.MediaControl.MediaLoaded += () =>
			{
				timelineSlider.Maximum = presentation.MediaControl.Duration.TotalMilliseconds;
				timelineSlider.IsSelectionRangeEnabled = true;
				timelineSlider.SelectionStart = media.OffsetStart.TotalMilliseconds;
				timelineSlider.SelectionEnd = presentation.MediaControl.Duration.TotalMilliseconds - media.OffsetEnd.TotalMilliseconds;
				timelineSlider.Value = media.OffsetStart.TotalMilliseconds;
				volumeSlider.Value = presentation.MediaControl.Volume;
				totalTimeLabel.Content = FormatTimeSpan(presentation.MediaControl.Duration);
				// TODO: Show presentation only when started (-> no preview would be available)?
				Controller.PresentationManager.CurrentPresentation = presentation;
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
					if (media.OffsetEnd.TotalMilliseconds > 0 && timelineSlider.Value >= timelineSlider.SelectionEnd)
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
			PlayState = PlayState.Stopped;
		}

		private void OnMouseDownPlayPauseMedia(object sender, RoutedEventArgs e)
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

		public void Close()
		{
			if (presentation != null && Controller.PresentationManager.CurrentPresentation == presentation)
				presentation.Close(); // TODO: really close (stop) the presentation when we're closing the control panel?
		}
	}
}
