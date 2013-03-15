using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WordsLive.AudioVideo;
using WordsLive.Core;
using WordsLive.Core.Songs;

namespace WordsLive.Songs
{
	public class SongPresentation : AwesomiumPresentation
	{
		private Song song;
		private Dictionary<SongSlide, int> slides = new Dictionary<SongSlide, int>();
		private SongDisplayController controller;
		private Image frontImage;
		private Image backImage;
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

		public bool ShowChords
		{
			get
			{
				return controller.ShowChords;
			}
			set
			{
				controller.ShowChords = value;
			}
		}

		public void Load(Song song)
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
				frontImage = new Image { Stretch = Stretch.Fill };
				backImage = new Image { Stretch = Stretch.Fill };

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
			

			this.IsTransparent = true;

			controller = new SongDisplayController(Control.Web, SongDisplayController.FeatureLevel.None);

			controller.SongLoaded += (sender, args) =>
			{
				OnFinishedLoading();
			};

			Control.Web.IsDirtyChanged += new EventHandler(web_IsDirtyChanged);

			currentSlideIndex = -1;

			this.Control.Web.JSConsoleMessageAdded += (obj, target) =>
			{
			    System.Windows.MessageBox.Show("JS error in line "+target.LineNumber+": "+target.Message);
			};

			controller.Load(this.song);
		}

		void web_IsDirtyChanged(object sender, EventArgs e)
		{
			Control.RenderWebView();
			if (!Control.Web.IsDirty)
				UpdateSlide();
		}

		public event EventHandler FinishedLoading;

		protected void OnFinishedLoading()
		{
			if (FinishedLoading != null)
				FinishedLoading(this, EventArgs.Empty);
		}

		private void GotoSlide(int index, bool update = false)
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
			base.Close();
			if (videoBackground != null)
				videoBackground.Destroy();
		}
	}
}
