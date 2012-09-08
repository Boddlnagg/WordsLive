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

	public class MediaOrderList : BindingList<MediaOrderItem>, INotifyPropertyChanged
	{
		private MediaOrderItem activeItem;

		public MediaOrderItem ActiveItem
		{
			get
			{
				return activeItem;
			}
			set
			{
				activeItem = value;
				OnPropertyChanged("ActiveItem");
				OnPropertyChanged("ActiveMedia");
				OnActiveItemChanged();
			}
		}

		public Media ActiveMedia
		{
			get
			{
				if (ActiveItem == null)
					return null;
				else
					return ActiveItem.Data;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event EventHandler ActiveItemChanged;

		public void OnActiveItemChanged()
		{
			if (ActiveItemChanged != null)
				ActiveItemChanged(this, EventArgs.Empty);
		}

		public void Add(Media media)
		{
			this.Add(CreateItem(media));
		}

		public void Insert(int index, Media media)
		{
			this.Insert(index, CreateItem(media));
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

		internal MediaOrderItem Replace(MediaOrderItem oldItem, Media newItem)
		{
			bool raiseListChangedEvents = this.RaiseListChangedEvents;
			int i = this.IndexOf(oldItem);
			MediaOrderItem item;

			try
			{
				this.RaiseListChangedEvents = false;
				this.Remove(oldItem);
				item = CreateItem(newItem);
				this.Insert(i, item);
			}
			finally
			{
				this.RaiseListChangedEvents = raiseListChangedEvents;
			}

			this.ResetBindings();

			return item;
		}

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
