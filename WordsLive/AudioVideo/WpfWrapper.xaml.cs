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
using System.Windows.Media;

namespace WordsLive.AudioVideo
{
	public partial class WpfWrapper : BaseMediaControl
	{
		MediaPlayer player;

		public WpfWrapper()
		{
			InitializeComponent();
			player = new MediaPlayer();
			var vd = new VideoDrawing();
			vd.Player = player;
			vd.Rect = new Rect(0, 0, 1, 1);
			var db = new DrawingBrush();
			db.Drawing = vd;
			video.Fill = db;
		}

		public override TimeSpan Duration
		{
			get { return player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : TimeSpan.Zero; }
		}

		public override int Position
		{
			get
			{
				return (int)player.Position.TotalMilliseconds;
			}
			set
			{
				player.Position = new TimeSpan(0, 0, 0, 0, value);
			}
		}

		public override int Volume
		{
			get
			{
				return (int)(player.Volume * 100);
			}
			set
			{
				player.Volume = value / 100.0;
			}
		}

		public override bool Loop { get; set; }
		public override bool Autoplay { get; set; }

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

		public override void Load(Uri uri)
		{
			player.MediaFailed += (sender, args) =>
			{
				OnMediaFailed();
			};

			player.MediaOpened += (sender, args) =>
			{
				// set correct size
				video.Width = player.NaturalVideoWidth;
				video.Height = player.NaturalVideoHeight;

				player.Volume = 1;

				if (!Autoplay)
					player.Pause();
				else
					rect.Visibility = System.Windows.Visibility.Hidden;

				OnMediaLoaded();
			};

			player.MediaEnded += (sender, args) =>
			{
				if (!Loop)
				{
					Stop();
					OnPlaybackEnded();
				}
				else
				{
					player.Stop();
					OnSeekStart();
					player.Play();
				}
			};

			player.Open(uri);
			player.Play();
		}

		public override void Play()
		{
			player.Play();
			rect.Visibility = System.Windows.Visibility.Hidden;
		}

		public override void Pause()
		{
			player.Pause();
		}

		public override void Stop()
		{
			player.Stop();
			OnSeekStart();
			rect.Visibility = System.Windows.Visibility.Visible;
		}

		public override void Destroy()
		{
			player.Stop();
		}
	}
}
