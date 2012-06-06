using System.Collections.Generic;
using Words.Core;

namespace Words.MediaOrderList
{
	public class MediaOrderList : ActivatableBindingList<MediaOrderItem>
	{
		public void Add(Media media)
		{
			this.Add(CreateItem(media));
		}

		public void Insert(int index, Media media)
		{
			this.Insert(index, CreateItem(media));
		}

		internal void ReplaceActiveBy(Media newItem)
		{
			for (int i = 0; i < this.Items.Count; i++)
			{
				if (this.Items[i].IsActive)
				{
					// For some reason uncommenting the code does mess up the IsActive-bold-marker in the list
					// (the active item is not marked anymore after the replace)

					//bool raiseListChangedEvents = this.RaiseListChangedEvents;
					//try
					//{
						//this.RaiseListChangedEvents = false;
						
						this.ActiveItem = null;
						RemoveIgnoreActivated = true;
						this.RemoveItem(i);
						RemoveIgnoreActivated = false;
						if (i > this.Count)
							this.Add(CreateItem(newItem));
						else
							this.Insert(i, CreateItem(newItem));


						//this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, i));

						this.ActiveItem = this.Items[i];
					//}
					//finally
					//{
						//this.RaiseListChangedEvents = raiseListChangedEvents;
					//}

					break;
				}
			}
		}

		public IEnumerable<Media> Export()
		{
			foreach (var i in this.Items)
			{
				yield return i.Data.Data;
			}
		}

		private bool CanActivate(Media media)
		{
			if (media is UnsupportedMedia)
			{
				System.Windows.MessageBox.Show("Die Datei " + media.File + " kann nicht angezeigt werden, da das Format nicht unterstützt wird.");
				return false;
			}
			else if (media is FileNotFoundMedia)
			{
				System.Windows.MessageBox.Show("Die Datei " + media.File + " existiert nicht.");
				return false;
			}
			else
			{
				return true;
			}
		}

		private MediaOrderItem CreateItem(Media media)
		{
			return new MediaOrderItem(media, CanActivate);
		}
	}
}
