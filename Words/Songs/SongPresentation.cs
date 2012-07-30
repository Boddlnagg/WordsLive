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

		public void Load(Song song, bool showChords)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			string video = null; // @"C:\Users\Patrick\Documents\Visual Studio 2010\Projects\Words\bin\Debug\Test\Kerze.mp4";

			this.song = song;

			base.Load(false, true);

			DoubleAnimation ani = new DoubleAnimation { From = 0.0, To = 1.0 };
			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
			Storyboard.SetTarget(ani, Control.ForegroundGrid);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));

			if (video == null)
			{
				frontImage = new Image { Stretch = Stretch.Fill };
				backImage = new Image { Stretch = Stretch.Fill };

				this.Control.BackgroundGrid.Children.Add(backImage);
				this.Control.ForegroundGrid.Children.Add(frontImage);
			}
			else
			{
				videoBackground = new AudioVideo.WpfWrapper(); // TODO: use VlcWrapper (configurable)
				
				videoBackground.Autoplay = true;
				videoBackground.Loop = true;
				videoBackground.Load(video);

				var brush = new System.Windows.Media.VisualBrush(videoBackground);
				videoBackgroundClone = new System.Windows.Shapes.Rectangle();
				videoBackgroundClone.Fill = brush;

				this.Control.ForegroundGrid.Children.Add(videoBackground);
				this.Control.BackgroundGrid.Children.Add(videoBackgroundClone);
			}
			

			this.IsTransparent = true;

			controller = new SongDisplayController(Control.Web);

			this.Control.Web.LoadCompleted += (sender, args) =>
			{
				controller.UpdateCss(this.song, this.Area.WindowSize.Width);
				controller.PreloadImages(from bg in this.song.Backgrounds where bg.IsImage select Path.Combine(MediaManager.BackgroundsDirectory, bg.ImagePath));
			};

			Control.Web.IsDirtyChanged += new EventHandler(web_IsDirtyChanged);

			controller.ShowChords = showChords;

			this.Area.WindowSizeChanged += OnWindowSizeChanged;

			currentSlideIndex = -1;

			controller.ImagesLoaded += OnFinishedLoading;

			this.Control.Web.JSConsoleMessageAdded += (obj, target) =>
			{
			    System.Windows.MessageBox.Show("JS error in line "+target.LineNumber+": "+target.Message);
			};

			this.Control.Web.LoadFile("song.html");
		}

		void web_IsDirtyChanged(object sender, EventArgs e)
		{
			Control.RenderWebView();
			if (!Control.Web.IsDirty)
				UpdateSlide();
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

			//controller.GotoSlide(song, slide, showSource, showCopyright, /*update ? 0 : Properties.Settings.Default.SongSlideTransition*/ 0, update);
			//controller.UpdateSlideWithCallback(song, slide, showSource, showCopyright, UpdateForeground);
			controller.UpdateSlide(song, slide, false);
			controller.ShowSource(showSource);
			controller.ShowCopyright(showCopyright);

			if (videoBackground == null) // only change backgrounds if we're not using a video background
			{
				SongBackground bg;

				if (slide != null)
					bg = song.Backgrounds[slide.BackgroundIndex];
				else
					bg = song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0];

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
			storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.SongSlideTransition);
			storyboard.Begin(this.Control.BackgroundGrid);
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
