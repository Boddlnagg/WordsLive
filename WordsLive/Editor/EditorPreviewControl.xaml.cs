using System.Windows.Controls;
using System.Linq;
using Awesomium.Windows.Controls;
using WordsLive.Core.Songs;
using WordsLive.Presentation.Wpf;
using WordsLive.Songs;
using System;
using System.IO;
using WordsLive.Core;
using System.Windows;

namespace WordsLive.Editor
{
	public partial class EditorPreviewControl : UserControl
	{
		SongDisplayController controller;

		public event EventHandler FinishedLoading;

		public EditorPreviewControl()
		{
			InitializeComponent();

			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				return;

			Init();
		}

		private void Init()
		{
			AwesomiumManager.Register(Web);

			Web.Crashed += OnWebViewCrashed;

			controller = new SongDisplayController(Web);
			controller.ShowChords = true;

			controller.ImagesLoaded += (sender, args) => OnFinishedLoading();

			Web.DeferInput();

			if (Song != null) // if this is not the first Init(), probably a song has already be loaded and must be reloaded
			{
				Web.LoadCompleted += (sender, args) => Load();
			}

			Web.LoadFile("song.html");

			// this will never be called when using fixed size
			//Web.SizeChanged += (sender, args) =>
			//{
			//    controller.UpdateCss(song, (int)Web.ActualWidth);
			//};
		}

		void OnWebViewCrashed(object sender, EventArgs e)
		{
			AwesomiumManager.Close(Web);
			var newWeb = new WebControl()
			{
				Width = 800,
				Height = 600,
			};

			webControlContainer.Child = newWeb;
			Web = newWeb;
			Init();
		}

		protected void OnFinishedLoading()
		{
			if (FinishedLoading != null)
				FinishedLoading(this, EventArgs.Empty);
		}

		private Song song;

		public Song Song
		{
			get
			{
				return song;
			}
			set
			{
				this.song = value;

				if (!Web.IsDomReady)
				{
					Web.LoadCompleted += (sender, args) => Load();
				}
				else
				{
					Load();
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
				UpdateStyle();
				Update();
			}
		}

		private void Load()
		{
			// TODO: previews don't load correctly when more than one file is opened simultaneously
			UpdateStyle();
			controller.PreloadImages(from bg in song.Backgrounds where bg.IsImage select Path.Combine(MediaManager.BackgroundsDirectory, bg.ImagePath));
		}

		private bool isFirstSelected;
		public bool IsFirstSelected
		{
			get
			{
				return isFirstSelected;
			}
			set
			{
				if (value == isFirstSelected)
					return;
				
				isFirstSelected = value;
				UpdateSourceCopyright();
			}
		}

		private bool isLastSelected;
		public bool IsLastSelected
		{
			get
			{
				return isLastSelected;
			}
			set
			{
				if (value == isLastSelected)
					return;

				isLastSelected = value;
				UpdateSourceCopyright();
			}
		}

		private void UpdateSourceCopyright()
		{
			if (!(element is SongSlide))
				return;

			bool showSource = ((song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.AllSlides ||
				(song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.FirstSlide && IsFirstSelected) ||
				(song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.LastSlide && IsLastSelected)));

			bool showCopyright = ((song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.AllSlides ||
				(song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.FirstSlide && IsFirstSelected) ||
				(song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.LastSlide && IsLastSelected)));

			controller.SetCopyright(song.Copyright);
			controller.ShowCopyright(showCopyright);
			controller.SetSource(song.Sources[0]);
			controller.ShowSource(showSource);
		}

		private ISongElement element;

		public ISongElement Element
		{
			get
			{
				return element;
			}
			set
			{
				element = value;
				Update();
			}
		}

		public void UpdateStyle()
		{
			controller.UpdateCss(song, (int)Web.ActualWidth);
		}

		public void Update()
		{
			if (element == null)
				return;

			if (element is SongSlide)
			{
				controller.UpdateSlide(song, (element as SongSlide));
				// TODO: remove event handler when element changes
				(element as SongSlide).PropertyChanged += (sender, args) =>
				{
					if (element != sender)
						return;

					if (args.PropertyName == "Text" || args.PropertyName == "Translation" ||args.PropertyName == "Background" || args.PropertyName == "Size")
						controller.UpdateSlide(song, (element as SongSlide));

					if (args.PropertyName == "HasTranslation" || args.PropertyName == "HasChords")
					{
						UpdateStyle();
					}

				};

				UpdateSourceCopyright();
			}
			else if (element is Nodes.CopyrightNode)
			{
				switch (song.Formatting.CopyrightDisplayPosition)
				{
					case MetadataDisplayPosition.AllSlides:
					case MetadataDisplayPosition.FirstSlide:
						controller.UpdateSlide(song, song.FirstSlide);
						break;
					case MetadataDisplayPosition.LastSlide:
						controller.UpdateSlide(song, song.LastSlide);
						break;
					case MetadataDisplayPosition.None:
						controller.UpdateSlide(song, new SongSlide(song));
						break;
				}

				controller.SetCopyright(song.Copyright);
				controller.ShowCopyright(true);
				controller.ShowSource(false);

				// TODO: remove event handler when element changes
				element.Root.PropertyChanged += (sender, args) =>
				{
					if (element.Root != sender)
						return;

					if (args.PropertyName == "Copyright")
						controller.SetCopyright(song.Copyright);
					
					// TODO: UpdateStyle() on song.Formatting change
				};
			}
			else if (element is Nodes.SourceNode)
			{
				controller.UpdateSlide(song, song.FirstSlide);
				controller.SetSource(song.Sources[0]);
				controller.ShowSource(true);
				controller.ShowCopyright(false);

				// TODO: remove event handler when element changes
				song.Sources[0].PropertyChanged += (sender, args) =>
				{
					if (song.Sources[0] != sender)
						return;

					if (args.PropertyName == "Songbook" || args.PropertyName == "Number")
						controller.SetSource(song.Sources[0]);

					// TODO: UpdateStyle() on song.Formatting change
				};
			}
			else
			{
				controller.UpdateSlide(song, null);
				controller.ShowCopyright(false);
				controller.ShowSource(false);
			}
		}

		internal void Cleanup()
		{
			AwesomiumManager.Close(Web);
		}
	}
}
