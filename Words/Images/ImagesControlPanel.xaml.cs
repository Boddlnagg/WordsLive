using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Words.Utils;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

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
				pres.ShowImage((ImagesMedia.ImageInfo)slideListView.SelectedItem);
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

		private void slideListView_DragEnterOrOver(object sender, System.Windows.DragEventArgs e)
		{
			int i = GetInsertionIndex(e.GetPosition(slideListView));
			this.RemoveInsertionAdorner();

			if (i >= 0)
			{
				this.CreateInsertionAdorner(slideListView.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement);
				if (e.Data.GetData(typeof(ImagesMedia.ImageInfo)) != null)
					e.Effects = DragDropEffects.Move;
				else
					e.Effects = DragDropEffects.Copy;
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
				if (e.Data.GetData(typeof(ImagesMedia.ImageInfo)) != null)
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
						// TODO: check validity (image format)
						media.Images.Insert(i + 1, new ImagesMedia.ImageInfo(file));
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

			return index;
		}

		InsertionAdorner insertionAdorner;
		int oldIndex;
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
				var selectedItem = (ImagesMedia.ImageInfo)slideListView.Items[oldIndex];
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
			if (e.Command == ApplicationCommands.Delete)
			{
				e.CanExecute = slideListView.SelectedItem != null;
			}
		}


		private void CommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Delete)
			{
				var item = (ImagesMedia.ImageInfo)slideListView.SelectedItem;
				this.media.Images.Remove(item);
			}
		}
	}
}
