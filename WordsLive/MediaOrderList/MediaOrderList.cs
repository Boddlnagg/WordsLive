/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WordsLive.Core;

namespace WordsLive.MediaOrderList
{
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
				if (value == null || CanActivate(value))
				{
					activeItem = value;
					OnPropertyChanged("ActiveItem");
					OnPropertyChanged("ActiveMedia");
					OnActiveItemChanged();
				}
				else
				{
					OnPropertyChanged("ActiveItem");
				}
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

		public MediaOrderItem Add(Media media)
		{
			var item = CreateItem(media);
			this.Add(item);
			return item;
		}

		public MediaOrderItem Insert(int index, Media media)
		{
			var item = CreateItem(media);
			this.Insert(index, item);
			return item;
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
			
			this.ResetItem(i);
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, i, i)); // workaround for a bug in ListBox

			return item;
		}

		internal void ReplaceActive(Media newData)
		{
			Controller.CurrentPanel = null;
			var newActive = Replace(ActiveItem, newData);
			if (CanActivate(newActive))
				ActiveItem = newActive;
			else
				ActiveItem = null;
		}

		public IEnumerable<Media> Export()
		{
			foreach (var i in this.Items)
			{
				yield return i.Data;
			}
		}

		public void Reload(MediaOrderItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (item == ActiveItem)
			{
				if (Controller.CurrentPanel != null && Controller.CurrentPanel.IsUpdatable)
				{
					item.ReloadMetadata();
					if (item.Data is FileNotFoundMedia) // file doesn't exist anymore?
					{
						ReplaceActive(item.Data);
					}
					else
					{
						MediaManager.LoadMedia(item.Data);
						Controller.CurrentPanel.Init(item.Data);
					}
				}
				else
				{
					var newData = MediaManager.LoadMediaMetadata(item.Data.Uri, item.Data.Options);
					ReplaceActive(newData);
				}
			}
			else
			{
				item.ReloadMetadata();
			}
		}

		private bool CanActivate(MediaOrderItem item)
		{
			if (item.Data is UnsupportedMedia)
			{
				MessageBox.Show(String.Format(Resources.Resource.vMsgActivateUnsupportedMedia, item.Data.Uri));
				return false;
			}
			else if (item.Data is FileNotFoundMedia)
			{
				MessageBox.Show(String.Format(Resources.Resource.vMsgActivateFileNotFoundMedia, item.Data.Uri));
				return false;
			}
			else
			{
				return true;
			}
		}

		private MediaOrderItem CreateItem(Media media)
		{
			return new MediaOrderItem(media);
		}
	}
}
