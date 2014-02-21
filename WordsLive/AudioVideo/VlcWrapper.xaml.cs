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
using Vlc.DotNet.Core.Medias;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public partial class VlcWrapper : BaseMediaControl
	{
		bool initialized = false;
		bool loop = false;
		LocationMedia media;
		float durationMilliseconds;

		static VlcWrapper()
		{
			VlcController.Init(); // init on first use
		}

		public VlcWrapper()
		{
			InitializeComponent();
		}

		private void OnMediaStateChange(MediaBase sender, Vlc.DotNet.Core.VlcEventArgs<Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States> e)
		{
			if (e.Data == Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Playing)
			{
				if (!initialized)
				{
					if (media.Duration.TotalMilliseconds > 0)
					{
						if (!Autoplay)
							vlc.Pause();
						else
							rect.Visibility = System.Windows.Visibility.Hidden;

						durationMilliseconds = (float)media.Duration.TotalMilliseconds;
						initialized = true;
						OnMediaLoaded();
					}
					else if (media.SubItems.Count > 0)
					{
						var subItems = media.SubItems;
						media.StateChanged -= OnMediaStateChange;
						subItems[0].StateChanged += OnMediaStateChange;
						media = subItems[0];
						vlc.Media = subItems[0];
						vlc.Play();
					}
					else
					{
						OnMediaFailed();
					}
				}
			}
		}

		public override void Load(Uri uri)
		{
			media = new LocationMedia(uri.AbsoluteUri);
			if (uri.Scheme == "dshow")
			{
				var nvc = uri.ParseQueryString();
				foreach (var key in nvc.AllKeys)
				{
					media.AddOption(key + "=" + nvc[key]);
				}
			}

			media.StateChanged += OnMediaStateChange;

			bool doLoop = false;

			vlc.EncounteredError += (sender, args) =>
			{
				OnMediaFailed();
			};

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

		public override event Action MediaFailed;

		protected void OnMediaFailed()
		{
			if (MediaFailed != null)
				MediaFailed();
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
				// TODO: this does not work for livestreams (like WebCam)
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
