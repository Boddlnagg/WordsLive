using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Words.Core;
using Words.Core.Songs;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using Words.AudioVideo;

namespace Words.Songs
{
	public class SongPresentation : AwesomiumPresentation
	{
		private Song song;
		private Dictionary<SongSlide, int> slides = new Dictionary<SongSlide, int>();
		private SongDisplayController controller;
		private Image frontImage;
		private Image backImage;
		private BaseMediaControl videoBackground;
		private Storyboard storyboard;

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

		public void Load(Song song, bool showChords)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			this.song = song;

			base.Load(false);

			frontImage = new Image { Stretch = Stretch.Fill };
			backImage = new Image { Stretch = Stretch.Fill };

			DoubleAnimation ani = new DoubleAnimation { From = 0.0, To = 1.0};
			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
			Storyboard.SetTarget(ani, frontImage);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));

			this.BackgroundGrid.Children.Add(backImage);
			this.BackgroundGrid.Children.Add(frontImage);
			
			/*
			videoBackground = new AudioVideo.VlcWrapper();
			this.BackgroundGrid.Children.Add(videoBackground);
			videoBackground.Autoplay = true;
			videoBackground.Loop = true;
			videoBackground.Load(@"C:\Users\Patrick\Documents\Visual Studio 2010\Projects\Words\bin\Debug\Test\Kerze.mp4");
			*/

			this.IsTransparent = true;

			this.Control.LoadCompleted += (sender, args) =>
			{
				controller.UpdateCss(this.song, this.Area.WindowSize.Width);
				controller.PreloadImages(from bg in this.song.Backgrounds where bg.IsImage select Path.Combine(MediaManager.BackgroundsDirectory, bg.ImagePath));
			};

			controller = new SongDisplayController(Control);
			controller.ShowChords = showChords;

			this.Area.WindowSizeChanged += OnWindowSizeChanged;

			currentSlideIndex = -1;

			controller.ImagesLoaded += OnFinishedLoading;

			this.Control.JSConsoleMessageAdded += (obj, target) => {
			    System.Windows.MessageBox.Show("JS error in line "+target.LineNumber+": "+target.Message);
			};

			this.Control.LoadFile("song.html");
		}

		private void OnWindowSizeChanged(object sender, EventArgs args)
		{	 
			controller.UpdateCss(this.song, this.Area.WindowSize.Width);
			GotoSlide(currentSlideIndex, true);
		}

		public event EventHandler FinishedLoading;

		protected void OnFinishedLoading(object sender, EventArgs args)
		{
			if (FinishedLoading != null)
				FinishedLoading(this, EventArgs.Empty);
		}

		private void GotoSlide(int index, bool update = false)
		{
			int maxSlideIndex = (from partName in song.Order select song.FindPartByName(partName).Slides.Count).Sum();

			bool showSource = ((song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.AllSlides ||
				(index == 1 && song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.FirstSlide) ||
				(index == maxSlideIndex && song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.LastSlide)));

			bool showCopyright = ((song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.AllSlides ||
				(index == 1 && song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.FirstSlide) ||
				(index == maxSlideIndex && song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.LastSlide)));

			if (showSource)
			{
				controller.SetSource(this.song.Sources[0]);
			}

			if (showCopyright)
			{
				controller.SetCopyright(this.song.Copyright);
			}

			var slide = FindSlideByIndex(index);

			controller.GotoSlide(song, slide, showSource, showCopyright, update ? 0 : Properties.Settings.Default.SongSlideTransition, update);

			if (videoBackground == null) // only change backgrounds if we're not using a video background
			{
				SongBackground bg;

				if (slide != null)
					bg = song.Backgrounds[slide.BackgroundIndex];
				else
					bg = song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0];

				ChangeBackground(bg);
			}
		}

		private void ChangeBackground(SongBackground bg)
		{
			//controller.ChangeBackground(bg, Properties.Settings.Default.SongSlideTransition);

			backImage.Source = frontImage.Source;
			frontImage.Source = SongBackgroundToImageSourceConverter.CreateBackgroundSource(bg);
			storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.SongSlideTransition);
			storyboard.Begin(this.BackgroundGrid);
		}

		private SongSlide FindSlideByIndex(int index)
		{
			int i = 1;
			foreach (var partName in song.Order)
			{
				SongPart part = song.FindPartByName(partName);
				foreach (var slide in part.Slides)
				{
					if (i++ == index)
					{
						return slide;
					}
				}
			}

			return null;
		}

		public override void Close()
		{
			// TODO: seems to have memory leak -> do some memory profiling on this
			base.Close();
			if (videoBackground != null)
				videoBackground.Destroy();

			this.Area.WindowSizeChanged -= OnWindowSizeChanged; // is this needed?
		}
	}
}
