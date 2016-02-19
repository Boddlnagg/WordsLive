/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using WordsLive.Slideshow.Presentation;
using WordsLive.Resources;
using System.Windows.Threading;

namespace WordsLive.Slideshow
{
	[TargetMedia(typeof(SlideshowMedia))]
	public partial class SlideshowControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		private SlideshowMedia media;
		private ISlideshowPresentation pres;
		private ControlPanelLoadState loadState;
		private DispatcherTimer autoAdvanceTimer;

		// TODO: don't save this in global setting, but per-presentation instead (same for images)
		public bool AutoAdvance
		{
			get
			{
				return Properties.Settings.Default.ImagesEnableAutoAdvance;
			}
			set
			{
				if (value != Properties.Settings.Default.ImagesEnableAutoAdvance)
				{
					Properties.Settings.Default.ImagesEnableAutoAdvance = value;
					OnPropertyChanged("AutoAdvance");
					ResetAutoAdvanceTimer();
				}
			}
		}

		public uint AutoAdvanceSeconds
		{
			get
			{
				return Properties.Settings.Default.ImagesAutoAdvanceSeconds;
			}
			set
			{
				if (value != Properties.Settings.Default.ImagesAutoAdvanceSeconds)
				{
					if (value > 999)
						value = 999;
					Properties.Settings.Default.ImagesAutoAdvanceSeconds = value;
					OnPropertyChanged("AutoAdvanceSeconds");
					ResetAutoAdvanceTimer();
				}
			}
		}

		public SlideshowControlPanel()
		{
			InitializeComponent();

			LoadState = ControlPanelLoadState.Loading;
			autoAdvanceTimer = new DispatcherTimer();
			autoAdvanceTimer.Tick += (sender, args) =>
			{
				if (LoadState == ControlPanelLoadState.Loaded)
				{
					pres.NextStep();
				}
			};
		}

		public Control Control
		{
			get { return this; }
		}

		public Core.Media Media
		{
			get { return media; }
		}

		private void ResetAutoAdvanceTimer()
		{
			autoAdvanceTimer.Stop();
			if (AutoAdvance && AutoAdvanceSeconds > 0)
			{
				autoAdvanceTimer.Interval = new TimeSpan(0, 0, (int)AutoAdvanceSeconds);
				autoAdvanceTimer.Start();
			}
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
						// make sure that toolbar buttons are enabled
						CommandManager.InvalidateRequerySuggested();
						ResetAutoAdvanceTimer();
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
				this.Dispatcher.BeginInvoke(new Action(presentation_SlideChanged));
			};

			pres.ClosedExternally += (sender, args) =>
			{
				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					Controller.PresentationManager.CurrentPresentation = null;
					Controller.FocusMainWindow(false);
					System.Windows.MessageBox.Show(Resource.slideErrorMsgClosedExternally);
					Controller.ReloadActiveMedia();
				}));
			};
		}

		public void Init(Core.Media media)
		{
			if (media is SlideshowMedia)
			{
				this.media = (SlideshowMedia)media;
				pres = SlideshowPresentationFactory.CreatePresentation(this.media);
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
			if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.PageDown || e.Key == Key.Space)
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

		private void CommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.PreviousPage)
			{
				pres.PreviousStep();
			}
			else if (e.Command == NavigationCommands.NextPage)
			{
				pres.NextStep();
			}
		}

		private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.PreviousPage || e.Command == NavigationCommands.NextPage)
			{
				e.CanExecute = LoadState == ControlPanelLoadState.Loaded;
			}
		}

		private void Control_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Keyboard.Focus(slideListView);
		}
	}
}
