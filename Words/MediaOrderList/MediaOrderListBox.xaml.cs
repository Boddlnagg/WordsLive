using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Words.Core;
using Words.Utils;

namespace Words.MediaOrderList
{
	delegate Point GetPositionDelegate(IInputElement element);

	public partial class MediaOrderListBox : ListBox
	{
		int oldIndex = -1;

		private MediaOrderList list;

		private static readonly DependencyPropertyKey ActiveItemPropertyKey =
			DependencyProperty.RegisterReadOnly("ActiveItem", typeof(MediaOrderItem), typeof(MediaOrderListBox), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ActiveItemProperty = ActiveItemPropertyKey.DependencyProperty;

		public MediaOrderItem ActiveItem
		{
			get
			{
				return (MediaOrderItem)GetValue(ActiveItemProperty);
			}
			private set
			{
				SetValue(ActiveItemPropertyKey, value);
			}
		}

		//public void Init(MediaOrderList list)
		//{
		//    this.list = list;
		//    this.DataContext = this.list;
		//    this.list.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		//    {
		//        if (list.ActiveItem == null)
		//            ActiveItem = null;
		//        else
		//            ActiveItem = list.ActiveItem.Data;
		//    };
		//}

		public MediaOrderListBox()
		{
			this.InitializeComponent();
			this.PreviewMouseMove += new MouseEventHandler(MediaOrderListBox_PreviewMouseMove);
			this.Drop += new DragEventHandler(MediaOrderListBox_Drop);
		}

		void MediaOrderListBox_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed)
				oldIndex = -1;

			if (oldIndex < 0)
				return;

			this.SelectedIndex = oldIndex;
			ActivatableItemContainer<MediaOrderItem> selectedItem = this.Items[oldIndex] as ActivatableItemContainer<MediaOrderItem>;

			if (selectedItem == null)
				return;

			// this will create the drag "rectangle"
			DragDropEffects allowedEffects = DragDropEffects.Move;
			if (DragDrop.DoDragDrop(this, selectedItem, allowedEffects) != DragDropEffects.None)
			{
				// The item was dropped into a new location,
				// so make it the new selected item.
				this.SelectedItem = selectedItem;
			}
		}

		void MediaOrderListBox_Drop(object sender, DragEventArgs e)
		{
			int index = this.GetCurrentIndex(e.GetPosition);

			// Data comes from list itself
			if (e.Data.GetData(typeof(MediaOrderItem)) != null)
			{
				return; // TODO!!

				//if (oldIndex < 0)
				//    return;

				//if (index == oldIndex)
				//    return;

				//MediaOrderItem movedItem = list[oldIndex];

				//if (index < 0)
				//    list.Move(new ActivatableItemContainer<MediaOrderItem>[] {movedItem} , Items.Count - oldIndex - 1);
				//else
				//    list.Move(new ActivatableItemContainer<MediaOrderItem>[] { movedItem }, index - oldIndex);

				//oldIndex = -1;
			}
			// Data comes from explorer
			else if (e.Data.GetData(DataFormats.FileDrop) != null)
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				IEnumerable<Media> result;

				if (files.Length < 1)
					return;

				if (files.Length == 1)
				{
					if (MediaManager.TryLoadPortfolio(files[0], out result))
						Controller.OpenPortfolio(files[0]);
					else
					{
						Media m = MediaManager.LoadMediaMetadata(files[0]);
						if (index < 0)
							list.Add(m);
						else
							list.Insert(index, m);
					}
				}
				else
				{
					foreach (var m in MediaManager.LoadMultipleMediaMetadata(files))
					{
						if (index < 0)
							list.Add(m);
						else
							list.Insert(index, m);
					}
				}
			}
		}

		void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			oldIndex = this.GetCurrentIndex(e.GetPosition);
		}

		void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			var data = (ActivatableItemContainer<MediaOrderItem>)this.ItemContainerGenerator.ItemFromContainer(sender as ListBoxItem);

			//list.ActiveItem = data;
		}
	}
}