using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Words.Utils;

namespace Words.Images
{
	[TargetMedia(typeof(ImagesMedia))]
	public partial class ImagesControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		private ImagesMedia media;
		private ImagesPresentation pres;
		private DispatcherTimer autoAdvanceTimer;
		private bool resetAutoAdvance = true;

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

		public FinishAction FinishAction
		{
			get
			{
				return Properties.Settings.Default.ImagesFinishAction;
			}
			set
			{
				Properties.Settings.Default.ImagesFinishAction = value;
				OnPropertyChanged("FinishAction");
			}
		}

		public ImagesControlPanel()
		{
			InitializeComponent();

			autoAdvanceTimer = new DispatcherTimer();
			autoAdvanceTimer.Tick += autoAdvanceTimer_Tick;
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

		void autoAdvanceTimer_Tick(object sender, EventArgs e)
		{
			resetAutoAdvance = false;
			ShowNext();
			resetAutoAdvance = true;
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
				this.DataContext = this;
				if (this.media.Images.Count > 0)
					this.slideListView.SelectedIndex = 0;
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
			UpdatePresentation();
		}

		private void UpdatePresentation()
		{
			if (slideListView.SelectedItem != null)
			{
				if (resetAutoAdvance)
					ResetAutoAdvanceTimer();

				pres.CurrentImage = (ImageInfo)slideListView.SelectedItem;
				this.Cursor = Cursors.Wait;
			}
			slideListView.ScrollIntoView(slideListView.SelectedItem);
		}

		private void slideListView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.PageDown)
			{
				ShowNext();
				e.Handled = true;
			}
			else if (e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.PageUp)
			{
				ShowPrevious();
				e.Handled = true;
			}
		}

		protected void ShowNext()
		{
			if (slideListView.SelectedIndex + 1 < slideListView.Items.Count)
			{
				slideListView.SelectedIndex++;
			}
			else // reached end
			{
				switch(FinishAction)
				{
					case FinishAction.Rerun:
						slideListView.SelectedIndex = 0;
						break;
					case FinishAction.NextMedia:
						Controller.TryActivateNext();
						break;
					case FinishAction.Blackscreen:
						Controller.PresentationManager.Status = PresentationStatus.Blackscreen;
						break;
					case FinishAction.Stop:
						AutoAdvance = false;
						break;
				}
			}
		}

		protected void ShowPrevious()
		{
			if (slideListView.SelectedIndex > 0)
				slideListView.SelectedIndex--;
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
			autoAdvanceTimer.Stop();
			autoAdvanceTimer.Tick -= autoAdvanceTimer_Tick;

			if (Controller.PresentationManager.CurrentPresentation != pres)
				pres.Close();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void slideListView_DragEnterOrOver(object sender, System.Windows.DragEventArgs e)
		{
			int i = GetInsertionIndex(e.GetPosition(slideListView));
			this.RemoveInsertionAdorner();

			if (i >= 0)
			{
				this.CreateInsertionAdorner(slideListView.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement);
				if (e.Data.GetData(typeof(ImageInfo)) != null)
					e.Effects = DragDropEffects.Move;
				else if (((string[])e.Data.GetData(DataFormats.FileDrop)).Where((f) => media.IsValidImageFile(f)).Any())
					e.Effects = DragDropEffects.Copy;
				else
					e.Effects = DragDropEffects.None;
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}

			e.Handled = true;
		}

		private void slideListView_DragLeave(object sender, DragEventArgs e)
		{
			this.RemoveInsertionAdorner();
		}

		private void slideListView_Drop(object sender, System.Windows.DragEventArgs e)
		{
			this.RemoveInsertionAdorner();

			int i = GetInsertionIndex(e.GetPosition(slideListView));

			if (i >= 0)
			{
				// Data comes from list itself
				if (e.Data.GetData(typeof(ImageInfo)) != null)
				{
					if (oldIndex < 0)
						return;

					if (i == oldIndex)
						return;

					media.Images.Move(oldIndex, i < oldIndex ? i + 1 : i);
					oldIndex = -1;
				}
				// Data comes from explorer
				else if (e.Data.GetData(DataFormats.FileDrop) != null)
				{
					string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
					foreach (string file in files)
					{
						if (media.IsValidImageFile(file))
						{
							media.Images.Insert(media.Images.Count > 0 ? i + 1 : 0, new ImageInfo(file));
						}
					}
				}
			}
		}

		private int GetInsertionIndex(Point p)
		{
			int index = slideListView.GetIndexAtPosition(p);
			if (index == -1 && slideListView.Items.Count > 0)
			{
				var h = slideListView.InputHitTest(p) as DependencyObject;
				if (h.FindVisualParent<ScrollBar, ListView>() != null)
					return -1;
				
				var container = slideListView.ItemContainerGenerator.ContainerFromIndex(0);
				var width = (container as ListViewItem).ActualWidth;
				p.X -= width;
				index = slideListView.GetIndexAtPosition(p);
				if (index == -1)
					index = slideListView.Items.Count - 1;
			}
			else if (index == -1)
			{
				index = 0;
			}

			return index;
		}

		InsertionAdorner insertionAdorner;
		int oldIndex = -1;
		Point startPoint;

		private void CreateInsertionAdorner(FrameworkElement targetItemContainer)
		{
			if (targetItemContainer != null)
			{
				// Here, I need to get adorner layer from targetItemContainer and not targetItemsControl.
				// This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
				// If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
				var adornerLayer = AdornerLayer.GetAdornerLayer(targetItemContainer);
				this.insertionAdorner = new InsertionAdorner(false, false, targetItemContainer, adornerLayer);
			}
		}

		private void RemoveInsertionAdorner()
		{
			if (this.insertionAdorner != null)
			{
				this.insertionAdorner.Detach();
				this.insertionAdorner = null;
			}
		}

		private void slideListView_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && oldIndex >= 0 && e.GetPosition(slideListView).ExceedsMinimumDragDistance(startPoint))
			{
				var selectedItem = (ImageInfo)slideListView.Items[oldIndex];
				DragDrop.DoDragDrop(this, selectedItem, DragDropEffects.Move);
			}
		}

		private void slideListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(slideListView);
			oldIndex = slideListView.GetIndexAtPosition(p);
			if (oldIndex >= 0)
				startPoint = p;
		}
		private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			ImageInfo img = e.Parameter as ImageInfo;

			if (e.Command == ApplicationCommands.Delete)
			{
				e.CanExecute = true;
			}
			else if (e.Command == CustomCommands.RotateLeft || e.Command == CustomCommands.RotateRight)
			{
				e.CanExecute = media.CanEdit && img.IsJpeg && (img != slideListView.SelectedItem || !pres.IsLoadingImage);
			}
			else if (e.Command == ApplicationCommands.Save)
			{
				e.CanExecute = media != null && media.CanSave;
			}

			e.Handled = true;
		}


		private void CommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ImageInfo img = e.Parameter as ImageInfo;

			if (e.Command == ApplicationCommands.Delete)
			{
				this.media.Images.Remove(img);
			}
			else if (e.Command == CustomCommands.RotateLeft)
			{
				img.RotateLeft();
				if (img == slideListView.SelectedItem)
					UpdatePresentation();
			}
			else if (e.Command == CustomCommands.RotateRight)
			{
				img.RotateRight();
				if (img == slideListView.SelectedItem)
					UpdatePresentation();
			}
			else if (e.Command == ApplicationCommands.Save)
			{
				this.media.Save();
			}
		}

		private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			slideListView.Focus();
			e.Handled = true;
		}
	}
}
