using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using WordsLive.Core.Songs;
using WordsLive.Songs;

namespace WordsLive.Editor
{
	public partial class EditorPreviewControl : UserControl
	{
		public static readonly DependencyProperty SongProperty = 
			DependencyProperty.Register("Song", typeof(Song), typeof(EditorPreviewControl), new PropertyMetadata(OnSongChanged));

		public Song Song
		{
			get { return (Song)GetValue(SongProperty); }
			set { SetValue(SongProperty, value); }
		}

		public static void OnSongChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			var control = (EditorPreviewControl)sender;

			if (args.OldValue != null)
			{
				(args.OldValue as Song).PropertyChanged -= control.Song_PropertyChanged;
				(args.OldValue as Song).Sources[0].PropertyChanged -= control.SongSource_PropertyChanged;
			}

			if (args.NewValue != null)
			{
				(args.NewValue as Song).PropertyChanged += control.Song_PropertyChanged;
				(args.NewValue as Song).Sources[0].PropertyChanged += control.SongSource_PropertyChanged;
			}

			if (args.NewValue != null && control.web.IsProcessCreated)
			{
				control.Load();
			}
		}

		void controller_SongLoaded(object sender, EventArgs e)
		{
			OnFinishedLoading();
		}

		SongDisplayController controller;

		public event EventHandler FinishedLoading;

		private global::Awesomium.Windows.Controls.WebControl web;

		public EditorPreviewControl()
		{
			InitializeComponent();

			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				return;

			Init();
		}

		private void Init()
		{
			web = new WebControl()
			{
				Width = 800,
				Height = 600,
			};

			webControlContainer.Child = web;
			
			web.Crashed += OnWebViewCrashed;
			web.ProcessInput = ViewInput.None;

			web.ProcessCreated += OnWebProcessCreated;

			if (Song != null && web.IsProcessCreated) // if this is not the first Init(), probably a song has already be loaded and must be reloaded
			{
				Load();
			}
		}

		void OnWebProcessCreated(object sender, WebViewEventArgs e)
		{
			if (Song != null)
			{
				Load();
			}
		}

		void OnWebViewCrashed(object sender, EventArgs e)
		{
			web.Dispose();
			Init();
		}

		protected void OnFinishedLoading()
		{
			if (FinishedLoading != null)
				FinishedLoading(this, EventArgs.Empty);
		}

		void Song_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (controller != null)
			{
				if (e.PropertyName == "Formatting" || e.PropertyName == "HasChords" || e.PropertyName == "HasTranslation")
				{
					controller.UpdateFormatting(Song.Formatting, Song.HasTranslation, Song.HasChords);
					Update(); // TODO: needed? (maybe for font size changes)
				}

				if (e.PropertyName == "Copyright")
					controller.SetCopyright(Song.Copyright);
			}
		}

		void SongSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// TODO: this is currently assuming that Song.Sources[0] does never change
			if (e.PropertyName == "Songbook" || e.PropertyName == "Number")
				controller.SetSource(Song.Sources[0]);
		}

		void SongSlide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Text" || e.PropertyName == "Translation" || e.PropertyName == "Background" || e.PropertyName == "Size")
				controller.ShowSlide((SongSlide)element);
		}

		public bool ShowChords
		{
			get
			{
				return showChords;
			}
			set
			{
				showChords = value;
				if (controller != null)
					controller.ShowChords = showChords;
			}
		}

		private bool showChords = true;

		private void Load()
		{
			controller = new SongDisplayController(web, SongDisplayController.FeatureLevel.Backgrounds);
			controller.ShowChords = showChords;
			controller.SongLoaded += controller_SongLoaded;
			controller.Load(Song);
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
			if (!(element is SongSlide) || Song == null)
				return;

			controller.ShowSource = ((Song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.AllSlides ||
				(Song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.FirstSlide && IsFirstSelected) ||
				(Song.Formatting.SourceDisplayPosition == MetadataDisplayPosition.LastSlide && IsLastSelected)));

			controller.ShowCopyright = ((Song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.AllSlides ||
				(Song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.FirstSlide && IsFirstSelected) ||
				(Song.Formatting.CopyrightDisplayPosition == MetadataDisplayPosition.LastSlide && IsLastSelected)));
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
				if (element is SongSlide)
				{
					(element as SongSlide).PropertyChanged -= SongSlide_PropertyChanged;
				}

				element = value;
				Update();
			}
		}

		public void Update()
		{
			if (element == null)
				return;

			if (element is SongSlide)
			{
				controller.ShowSlide(element as SongSlide);
				(element as SongSlide).PropertyChanged += SongSlide_PropertyChanged;
				UpdateSourceCopyright();
			}
			else if (element is Nodes.CopyrightNode)
			{
				switch (Song.Formatting.CopyrightDisplayPosition)
				{
					case MetadataDisplayPosition.AllSlides:
					case MetadataDisplayPosition.FirstSlide:
						controller.ShowSlide(Song.FirstSlide);
						break;
					case MetadataDisplayPosition.LastSlide:
						controller.ShowSlide(Song.LastSlide);
						break;
					case MetadataDisplayPosition.None:
						controller.ShowSlide(new SongSlide(Song));
						break;
				}

				controller.ShowCopyright = true;
				controller.ShowSource = false;
			}
			else if (element is Nodes.SourceNode)
			{
				switch (Song.Formatting.SourceDisplayPosition)
				{
					case MetadataDisplayPosition.AllSlides:
					case MetadataDisplayPosition.FirstSlide:
						controller.ShowSlide(Song.FirstSlide);
						break;
					case MetadataDisplayPosition.LastSlide:
						controller.ShowSlide(Song.LastSlide);
						break;
					case MetadataDisplayPosition.None:
						controller.ShowSlide(new SongSlide(Song));
						break;
				}
				controller.ShowSource = true;
				controller.ShowCopyright = false;
			}
			else
			{
				controller.GotoBlankSlide(Song.FirstSlide != null ? Song.FirstSlide.Background : Song.Backgrounds[0]);
			}
		}

		internal void Cleanup()
		{
			web.Dispose();
		}
	}
}
