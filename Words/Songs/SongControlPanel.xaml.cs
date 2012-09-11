using System;
using System.Windows.Controls;
using Words.Core;
using Words.Core.Songs;

namespace Words.Songs
{
	[TargetMedia(typeof(Song))]
	public partial class SongControlPanel : UserControl, IMediaControlPanel
	{
		private class SongSlideSelection
		{
			public string PartName { get; set; }
			public int PartIndex { get; set; }
			public int SlideIndex { get; set; }
		}

		private Song song;
		private bool finishedLoading;
		private SongPresentation presentation;
		private SongPresentation oldPresentation;

		public SongControlPanel()
		{
			this.InitializeComponent();
		}

		SongSlideSelection recoverSelection;

		public void Init(Media media)
		{
			Song s = (media as Song);

			if (s == null)
				throw new ArgumentException("media must be not null and a Song");

			bool updating = this.song != null; // whether we are updating

			this.song = s;

			if (SwapTextAndTranslation)
				DoSwapTextAndTranslation();

			Refresh(!updating);
		}

		void Refresh(bool firstTime)
		{
			if (!firstTime)
			{
				oldPresentation = presentation;
				oldPresentation.FinishedLoading -= pres_OnFinishedLoading;
			}

			finishedLoading = false;
			presentation = Controller.PresentationManager.CreatePresentation<SongPresentation>();
			presentation.FinishedLoading += pres_OnFinishedLoading;
			presentation.Load(this.song, ShowChords);

			if (!firstTime)
			{
				if (this.SlideListBox.SelectedItem != null)
				{
					var con = (SongSlideListBox.SongSlideContainer)this.SlideListBox.SelectedItem;
					recoverSelection = new SongSlideSelection { PartName = con.PartName, SlideIndex = con.SlideIndex, PartIndex = con.PartIndex };
				}
			}

			this.SlideListBox.Song = this.song;

			if (!firstTime && recoverSelection != null)
			{
				this.SlideListBox.SelectedIndex = RecoverSelectedSlide(recoverSelection);
			}
			else
			{
				this.SlideListBox.Loaded += (sender, args) => this.SlideListBox.SelectedIndex = -1;
			}
		}

		void pres_OnFinishedLoading(object sender, EventArgs args)
		{
			finishedLoading = true;
			recoverSelection = null;
			if (this.SlideListBox.SelectedIndex >= 0)
			{
				presentation.CurrentSlideIndex = this.SlideListBox.SelectedIndex;

				if (Controller.PresentationManager.CurrentPresentation != oldPresentation && oldPresentation != null)
					oldPresentation.Close();
				Controller.PresentationManager.CurrentPresentation = presentation;

				oldPresentation = null;
				
			}
		}

		private int RecoverSelectedSlide(SongSlideSelection selection)
		{
			int i = 1;
			int dist = -1;
			int slideCount = 1;
			int index = -1;
			SongPart part = null;

			foreach (var partName in song.Order)
			{
				if (partName == selection.PartName)
				{
					var newDist = Math.Abs(selection.PartIndex - i);
					if (i > selection.PartIndex && newDist > dist && dist >= 0)
						break;
					dist = newDist;
					part = song.FindPartByName(partName);
					index = slideCount;
				}
				slideCount += song.FindPartByName(partName).Slides.Count;
				i++;
			}

			if (part != null)
				return index + (selection.SlideIndex >= part.Slides.Count ? part.Slides.Count - 1 : selection.SlideIndex);
			else
				return 0;
		}

		public Control Control
		{
			get
			{
				return this;
			}
		}


		public Media Media
		{
			get
			{
				return song;
			}
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (finishedLoading && this.SlideListBox.SelectedIndex >= 0)
			{
				if (Controller.PresentationManager.CurrentPresentation != presentation)
					Controller.PresentationManager.CurrentPresentation = presentation;
				presentation.CurrentSlideIndex = this.SlideListBox.SelectedIndex;
			}
		}

		public bool IsUpdatable
		{
			get { return true; }
		}

		public ControlPanelLoadState LoadState
		{
			get { return ControlPanelLoadState.Loaded; }
		}

		private bool showChords;

		public bool ShowChords
		{
			get
			{
				return showChords;
			}
			set
			{
				if (value != showChords)
				{
					showChords = value;
					Refresh(false);
				}
			}
		}

		private bool swapTextAndTranslation;

		public bool SwapTextAndTranslation
		{
			get
			{
				return swapTextAndTranslation;
			}
			set
			{
				if (value != swapTextAndTranslation)
				{
					swapTextAndTranslation = value;
					DoSwapTextAndTranslation();
					Refresh(false); // reload
				}
			}
		}

		private void DoSwapTextAndTranslation()
		{
			foreach (var part in song.Parts)
			{
				foreach (var slide in part.Slides)
				{
					var tmp = slide.Translation;
					slide.Translation = slide.Text;
					slide.Text = tmp;
				}
			}
		}

		private void OnCanExecuteCommand(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.SwapTextAndTranslation)
			{
				e.CanExecute = this.song.HasTranslation; // swapping text and translation is only possible if the song has a translation
			}
		}

		private void OnExecuteCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.SwapTextAndTranslation)
			{
				// nothing to do (this is handled by the SwapTextAndTranslation property)
			}
		}

		public void Close()
		{
			if (Controller.PresentationManager.CurrentPresentation != presentation)
				presentation.Close();
		}
	}
}