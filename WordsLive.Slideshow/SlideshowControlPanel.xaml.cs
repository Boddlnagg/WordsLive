﻿using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace WordsLive.Slideshow
{
	[TargetMedia(typeof(SlideshowMedia))]
	public partial class SlideshowControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		private SlideshowMedia media;
		private ISlideshowPresentation pres;
		private ControlPanelLoadState loadState;

		public SlideshowControlPanel()
		{
			InitializeComponent();

			LoadState = ControlPanelLoadState.Loading;
		}

		public Control Control
		{
			get { return this; }
		}

		public Core.Media Media
		{
			get { return media; }
		}

		private void SetupEventListeners()
		{
			pres.Loaded += (sender, args) =>
			{
				this.Dispatcher.Invoke(new Action(() => {
					if (args.Success)
					{
						LoadState = ControlPanelLoadState.Loaded;
						Controller.PresentationManager.CurrentPresentation = pres;
						this.slideListView.DataContext = pres.Thumbnails;
						this.Focus();
					}
					else
					{
						LoadState = ControlPanelLoadState.Failed;
						Controller.PresentationManager.CurrentPresentation = null;
					}
				}));
			};

			pres.SlideIndexChanged += (sender, args) =>
			{
				this.Dispatcher.Invoke(new Action(presentation_SlideChanged));
			};

			pres.ClosedExternally += (sender, args) =>
			{
				this.Dispatcher.Invoke(new Action(() =>
				{
					Controller.PresentationManager.CurrentPresentation = null;
					Controller.FocusMainWindow();
					System.Windows.MessageBox.Show("Die Präsentation wurde unerwartet geschlossen. WordsLive hat die Anzeige schwarz geschaltet und wird versuchen, die Präsentation neu zu laden."); // TODO: localize
					Controller.ReloadActiveMedia();
				}));
			};
		}

		public void Init(Core.Media media)
		{
			if (media is SlideshowMedia)
			{
				this.media = (SlideshowMedia)media;
				pres = this.media.CreatePresentation();
				SetupEventListeners();
				pres.Load();
			}
			else
			{
				throw new ArgumentException("media is not a valid slideshow media.");
			}
		}

		private void presentation_SlideChanged()
		{
			this.slideListView.SelectedIndex = pres.SlideIndex;
		}

		private void slideListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (pres.SlideIndex != slideListView.SelectedIndex)
			{
				pres.GotoSlide(slideListView.SelectedIndex);
			}
			slideListView.ScrollIntoView(slideListView.SelectedItem);
		}

		private void slideListView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.PageDown)
			{
				pres.NextStep();
				e.Handled = true;
			}
			else if (e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.PageUp)
			{
				pres.PreviousStep();
				e.Handled = true;
			}
		}

		public bool IsUpdatable
		{
			get { return false; }
		}

		public ControlPanelLoadState LoadState
		{
			get
			{
				return loadState;
			}
			set
			{
				loadState = value;
				OnPropertyChanged("LoadState");
			}
		}

		public void Close()
		{
			if (Controller.PresentationManager.CurrentPresentation != pres)
				pres.Close();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
