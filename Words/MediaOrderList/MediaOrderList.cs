using System;
using System.Collections.Generic;
using Words.Core;
using System.ComponentModel;
using System.Linq;

namespace Words.MediaOrderList
{
	public class MediaNotificationEventArgs : EventArgs
	{
		public Media Media { get; set; }
	}

	public class MediaOrderList : BindingList<MediaOrderItem>
	{
		public event EventHandler ItemAdded;

		private void OnItemAdded()
		{
			if (ItemAdded != null)
				ItemAdded(this, new EventArgs());
		}

		public void Add(Media media)
		{
			this.Add(CreateItem(media));
			OnItemAdded();
		}

		public void Insert(int index, Media media)
		{
			this.Insert(index, CreateItem(media));
			OnItemAdded();
		}

		public MediaOrderItem Move(IEnumerable<MediaOrderItem> items, int delta)
		{
			if (delta == 0)
				return null;

			bool raiseListChangedEvents = this.RaiseListChangedEvents;

			items = delta > 0 ? items.OrderByDescending((item) => this.IndexOf(item)) : items.OrderBy((item) => this.IndexOf(item));

			var boundaryItem = items.First();

			if (delta > 0 && this.IndexOf(boundaryItem) >= this.Count - 1 || delta < 0 && this.IndexOf(boundaryItem) == 0)
				return null;

			try
			{
				this.RaiseListChangedEvents = false;

				foreach (var item in items)
				{
					int oldIndex = this.IndexOf(item);
					int index = oldIndex + delta;
					this.RemoveItem(oldIndex);
					this.InsertItem(index, item);
					this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, index, oldIndex));
				}
			}
			finally
			{
				this.RaiseListChangedEvents = raiseListChangedEvents;
			}

			return boundaryItem;
		}

		// TODO!!
		//internal void ReplaceActiveBy(Media newItem)
		//{
		//    for (int i = 0; i < this.Items.Count; i++)
		//    {
		//        if (this.Items[i].IsActive)
		//        {
		//            // For some reason uncommenting the code does mess up the IsActive-bold-marker in the list
		//            // (the active item is not marked anymore after the replace)

		//            //bool raiseListChangedEvents = this.RaiseListChangedEvents;
		//            //try
		//            //{
		//                //this.RaiseListChangedEvents = false;
						
		//                this.ActiveItem = null;
		//                RemoveIgnoreActivated = true;
		//                this.RemoveItem(i);
		//                RemoveIgnoreActivated = false;
		//                if (i > this.Count)
		//                    this.Add(CreateItem(newItem));
		//                else
		//                    this.Insert(i, CreateItem(newItem));


		//                //this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, i));

		//                this.ActiveItem = this.Items[i];
		//            //}
		//            //finally
		//            //{
		//                //this.RaiseListChangedEvents = raiseListChangedEvents;
		//            //}

		//            break;
		//        }
		//    }
		//}

		public IEnumerable<Media> Export()
		{
			foreach (var i in this.Items)
			{
				yield return i.Data;
			}
		}

		private bool CanActivate(Media media)
		{
			if (media is UnsupportedMedia)
			{
				OnNotifyTryOpenUnsupportedMedia(media);
				return false;
			}
			else if (media is FileNotFoundMedia)
			{
				OnNotifyTryOpenFileNotFoundMedia(media);
				return false;
			}
			else
			{
				return true;
			}
		}

		public event EventHandler<MediaNotificationEventArgs> NotifyTryOpenUnsupportedMedia;
		public event EventHandler<MediaNotificationEventArgs> NotifyTryOpenFileNotFoundMedia;

		protected void OnNotifyTryOpenUnsupportedMedia(Media media)
		{
			if (NotifyTryOpenUnsupportedMedia != null)
				NotifyTryOpenUnsupportedMedia(this, new MediaNotificationEventArgs { Media = media });
		}

		protected void OnNotifyTryOpenFileNotFoundMedia(Media media)
		{
			if (NotifyTryOpenFileNotFoundMedia != null)
				NotifyTryOpenFileNotFoundMedia(this, new MediaNotificationEventArgs { Media = media });
		}

		private MediaOrderItem CreateItem(Media media)
		{
			return new MediaOrderItem(media, CanActivate);
		}
	}
}
