using System.Windows.Controls;
using System.Linq;
using Awesomium.Windows.Controls;
using Words.Core.Songs;
using Words.Presentation.Wpf;
using Words.Songs;
using System;
using System.IO;
using Words.Core;
using System.Windows;

namespace Words.Editor
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
			if (!(node is SongNodeSlide))
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

		private SongNode node;

		public SongNode Node
		{
			get
			{
				return node;
			}
			set
			{
				node = value;
				Update();
			}
		}

		public void UpdateStyle()
		{
			controller.UpdateCss(song, (int)Web.ActualWidth);
		}

		public void Update()
		{
			if (node is SongNodeRoot)
			{
				controller.UpdateSlide(song, null);
				controller.ShowCopyright(false);
				controller.ShowSource(false);
			}
			else if (node is SongNodeSlide)
			{
				controller.UpdateSlide(song, (node as SongNodeSlide).Slide);
				(node as SongNodeSlide).PropertyChanged += (sender, args) =>
				{
					if (node != sender)
						return;

					if (args.PropertyName == "Text" || args.PropertyName == "Translation" ||args.PropertyName == "BackgroundIndex" || args.PropertyName == "FontSize")
						controller.UpdateSlide(song, (node as SongNodeSlide).Slide);

					if (args.PropertyName == "HasTranslation" || args.PropertyName == "HasChords")
					{
						UpdateStyle();
					}

				};

				UpdateSourceCopyright();
			}
			else if (node is SongNodeCopyright)
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
						controller.UpdateSlide(song, new SongSlide());
						break;
				}

				controller.SetCopyright(song.Copyright);
				controller.ShowCopyright(true);
				controller.ShowSource(false);
				(node as SongNodeCopyright).PropertyChanged += (sender, args) =>
				{
					if (node != sender)
						return;

					if (args.PropertyName == "Text")
						controller.SetCopyright(song.Copyright);
				};
			}
			else if (node is SongNodeSource)
			{
				controller.UpdateSlide(song, song.FirstSlide);
				controller.SetSource((node as SongNodeSource).Source);
				controller.ShowSource(true);
				controller.ShowCopyright(false);
				(node as SongNodeSource).PropertyChanged += (sender, args) =>
				{
					if (node != sender)
						return;

					if (args.PropertyName == "Songbook" || args.PropertyName == "Number")
						controller.SetSource((node as SongNodeSource).Source);
				};
			}
		}

		internal void Cleanup()
		{
			AwesomiumManager.Close(Web);
		}
	}
}
