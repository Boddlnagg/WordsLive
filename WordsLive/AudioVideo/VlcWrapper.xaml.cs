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

using LibVLCSharp.Shared;
using System;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public partial class VlcWrapper : BaseMediaControl
	{
		bool initialized = false;
		bool loop = false;
		LibVLCSharp.Shared.Media media;
		float durationMilliseconds;

		static VlcWrapper()
		{
			VlcController.Init(); // init on first use
		}

		public VlcWrapper()
		{
			InitializeComponent();
			vlc.MediaPlayer = new MediaPlayer(VlcController.LibVLC);
		}

		private void OnMediaStateChange(object sender, MediaStateChangedEventArgs e)
		{
			if (e.State == VLCState.Playing)
			{
				if (!initialized)
				{
					Controller.DispatchToMainWindow(() =>
					{
						if (media.Duration > 0)
						{
							if (!Autoplay)
								vlc.MediaPlayer.Pause();
							else
								rect.Visibility = System.Windows.Visibility.Hidden;

							durationMilliseconds = (float)media.Duration;
							initialized = true;
							OnMediaLoaded();
						}
						else if (media.SubItems.Count > 0)
						{
							var subItems = media.SubItems;
							media.StateChanged -= OnMediaStateChange;
							subItems[0].StateChanged += OnMediaStateChange;
							media = subItems[0];
							vlc.MediaPlayer.Media = subItems[0];
							vlc.MediaPlayer.Play();
						}
						else
						{
							OnMediaFailed();
						}
					});
				}
			}
		}

		public override void Load(Uri uri)
		{
			media = new LibVLCSharp.Shared.Media(VlcController.LibVLC, uri);
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

			vlc.MediaPlayer.EncounteredError += (sender, args) =>
			{
				OnMediaFailed();
			};

			vlc.MediaPlayer.EndReached += (sender, args) =>
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

			vlc.MediaPlayer.Playing += (sender, args) =>
			{
				if (loop && doLoop)
				{
					OnSeekStart();
				}
				doLoop = false;
			};

			vlc.MediaPlayer.Media = media;
			vlc.MediaPlayer.Play();
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
				//if (loop)
				//	vlc.MediaPlayer.PlaybackMode = Vlc.DotNet.Core.Interops.Signatures.LibVlc.MediaListPlayer.PlaybackModes.Loop;
				//else
				//	vlc.MediaPlayer.PlaybackMode = Vlc.DotNet.Core.Interops.Signatures.LibVlc.MediaListPlayer.PlaybackModes.Default;
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
				return TimeSpan.FromMilliseconds(media.Duration);
			}
		}

		public override int Position
		{
			get
			{
				return (int)(vlc.MediaPlayer.Position * durationMilliseconds);
			}
			set
			{
				if (durationMilliseconds != 0)
					vlc.MediaPlayer.Position = value / durationMilliseconds;
			}
		}


		public override int Volume
		{
			get
			{
				return vlc.MediaPlayer.Volume;
			}
			set
			{
				vlc.MediaPlayer.Volume = value;
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
			if (media.State == VLCState.Paused)
				vlc.MediaPlayer.Play();
			else
				vlc.MediaPlayer.Media = media;

			rect.Visibility = System.Windows.Visibility.Hidden;
		}

		public override void Pause()
		{
			if (media.State == VLCState.Playing)
				vlc.MediaPlayer.Pause();
		}

		public override void Stop()
		{
			if (media.State != VLCState.Paused) // correct?
			{
				// don't really stop, but pause and go back to beginning
				// TODO: this does not work for livestreams (like WebCam)
				vlc.MediaPlayer.Pause();
			}
			OnSeekStart();
			rect.Visibility = System.Windows.Visibility.Visible;
		}

		public override void Destroy()
		{
			vlc.MediaPlayer.Stop();
		}
	}
}
