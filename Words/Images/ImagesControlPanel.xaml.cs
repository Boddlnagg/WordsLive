using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Words.Images
{
	[TargetMedia(typeof(ImagesMedia))]
	public partial class ImagesControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		private ImagesMedia media;
		private ImagesPresentation pres;

		public ImagesControlPanel()
		{
			InitializeComponent();
		}

		public Control Control
		{
			get { return this; }
		}

		public Core.Media Media
		{
			get { return media; }
		}

		public void Init(Core.Media media)
		{
			if (media is ImagesMedia)
			{
				this.media = (ImagesMedia)media;
				pres = Controller.PresentationManager.CreatePresentation<ImagesPresentation>();
				pres.Load(this.media);
				Controller.PresentationManager.CurrentPresentation = pres;
				this.slideListView.DataContext = this.media.Images;
				this.Focus();
			}
			else
			{
				throw new ArgumentException("media is not a valid images media.");
			}
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
				return ControlPanelLoadState.Loaded;
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
