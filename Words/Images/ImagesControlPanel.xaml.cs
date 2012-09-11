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
				pres.LoadingFinished += pres_LoadingFinished;
				Controller.PresentationManager.CurrentPresentation = pres;
				this.slideListView.DataContext = this.media.Images;
			}
			else
			{
				throw new ArgumentException("media is not a valid images media.");
			}
		}

		void pres_LoadingFinished(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Arrow;
		}

		private void slideListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (slideListView.SelectedItem != null)
			{
				pres.ShowImage((System.IO.FileInfo)slideListView.SelectedItem);
				this.Cursor = Cursors.Wait;
			}
			slideListView.ScrollIntoView(slideListView.SelectedItem);
		}

		private void slideListView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.PageDown)
			{
				if (slideListView.SelectedIndex + 1 < slideListView.Items.Count)
					slideListView.SelectedIndex++;
				
				e.Handled = true;
			}
			else if (e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.PageUp)
			{
				if (slideListView.SelectedIndex > 0)
					slideListView.SelectedIndex--;
				e.Handled = true;
			}
		}

		public bool IsUpdatable
		{
			get { return false; } // TODO
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

		private void Remove_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var item = (System.IO.FileInfo)slideListView.SelectedItem;
			this.media.Images.Remove(item);
		}
	}
}
