using System;
using Vlc.DotNet.Core.Medias;

namespace WordsLive.AudioVideo
{
	public partial class VlcWrapper : BaseMediaControl
	{
		bool initialized = false;
		bool loop = false;
		MediaBase media;
		float durationMilliseconds;

		static VlcWrapper()
		{
			VlcController.Init(); // init on first use
		}

		public VlcWrapper()
		{
			InitializeComponent();
		}

		public override void Load(Uri uri)
		{
			media = new PathMedia(uri.AbsoluteUri);

			media.StateChanged += (sender, args) =>
			{
				if (media.State == Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Playing)
				{
					if (!initialized)
					{
						if (!Autoplay)
							vlc.Pause();
						else
							rect.Visibility = System.Windows.Visibility.Hidden;

						durationMilliseconds = (float)media.Duration.TotalMilliseconds;
						initialized = true;
						OnMediaLoaded();
					}
				}
			};

			bool doLoop = false;

			vlc.EndReached += (sender, args) =>
			{
				if (!loop)
				{
					Stop();
					OnPlaybackEnded();
				}
				else
				{
					doLoop = true;
				}
			};

			vlc.Playing += (sender, args) =>
			{
				if (loop && doLoop)
				{
					OnSeekStart();
				}
				doLoop = false;
			};

			media.MediaSubItemAdded += (sender, args) =>
			{
				// TODO: support streaming (need to listen to more events etc)
				vlc.Media = args.Data;
				vlc.Play();
			};

			vlc.Media = media;
		}

		public override bool Loop
		{
			get
			{
				return loop;
			}
			set
			{
				loop = value;
				if (loop)
					vlc.PlaybackMode = Vlc.DotNet.Core.Interops.Signatures.LibVlc.MediaListPlayer.PlaybackModes.Loop;
				else
					vlc.PlaybackMode = Vlc.DotNet.Core.Interops.Signatures.LibVlc.MediaListPlayer.PlaybackModes.Default;
			}
		}

		public override bool Autoplay
		{
			get;
			set;
		}

		public override TimeSpan Duration
		{
			get
			{
				return media.Duration;
			}
		}

		public override int Position
		{
			get
			{
				return (int)(vlc.Position * durationMilliseconds);
			}
			set
			{
				if (durationMilliseconds != 0)
					vlc.Position = value / durationMilliseconds;
			}
		}


		public override int Volume
		{
			get
			{
				return vlc.AudioProperties.Volume;
			}
			set
			{
				vlc.AudioProperties.Volume = value;
			}
		}

		public override event Action MediaLoaded;

		protected void OnMediaLoaded()
		{
			if (MediaLoaded != null)
				MediaLoaded();
		}

		public override event Action PlaybackEnded;

		protected void OnPlaybackEnded()
		{
			if (PlaybackEnded != null)
				PlaybackEnded();
		}

		public override event Action SeekStart;

		protected void OnSeekStart()
		{
			if (SeekStart != null)
				SeekStart();
		}

		public override void Play()
		{
			if (media.State == Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Paused)
				vlc.Play();
			else
				vlc.Media = media;

			rect.Visibility = System.Windows.Visibility.Hidden;
		}

		public override void Pause()
		{
			if (media.State == Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Playing)
				vlc.Pause();
		}

		public override void Stop()
		{
			if (!vlc.IsPaused)
			{
				// don't really stop, but pause and go back to beginning
				vlc.Pause();
			}
			OnSeekStart();
			rect.Visibility = System.Windows.Visibility.Visible;
		}

		public override void Destroy()
		{
			vlc.Stop();
		}
	}
}
