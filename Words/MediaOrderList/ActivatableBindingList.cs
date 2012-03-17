using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Linq;

namespace Words.MediaOrderList
{
	public class ActivatableBindingList<T> : BindingList<ActivatableItemContainer<T>>, INotifyPropertyChanged where T : IActivatable
	{
		/// <summary>
		/// When false, removing the currently activated item will set ActiveItem to null.
		/// </summary>
		protected bool RemoveIgnoreActivated { get; set; }
		
		public ActivatableItemContainer<T> Move(IEnumerable<ActivatableItemContainer<T>> items, int delta)
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
					RemoveIgnoreActivated = true;
					this.RemoveItem(oldIndex);
					RemoveIgnoreActivated = false;
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
		
		private ActivatableItemContainer<T> activeItem;
		
		public ActivatableItemContainer<T> ActiveItem
		{
			get
			{
				return activeItem;
			}
			set
			{
				ActivatableItemContainer<T> prevActive = activeItem;

				if (value != null && !value.Data.Activate())
				{
					return;
				}

				activeItem = value;

				if (activeItem != null)
					activeItem.NotifyActivatedChanged();
				
				if (prevActive != null)
					prevActive.NotifyActivatedChanged();
				
				OnPropertyChanged("ActiveItem");
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		protected override void RemoveItem(int index)
		{
			ActivatableItemContainer<T> item = this[index];
			base.RemoveItem(index);
			if (!RemoveIgnoreActivated && item == this.ActiveItem)
				this.ActiveItem = null;
		}
		
		public void Add(T item)
		{
			this.Add(new ActivatableItemContainer<T>(this, item));
		}
		
		public void Insert(int index, T item)
		{
			this.Insert(index, new ActivatableItemContainer<T>(this, item));
		}
	}
}
