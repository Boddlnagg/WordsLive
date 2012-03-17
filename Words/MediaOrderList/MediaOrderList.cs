using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;
using System.ComponentModel;

namespace Words.MediaOrderList
{
    public class MediaOrderList : ActivatableBindingList<MediaOrderItem>
    {
        public void Add(Media media)
        {
            this.Add(new MediaOrderItem(media));
        }

        public IEnumerable<Media> Export()
        {
            foreach (var i in this.Items)
            {
                yield return i.Data.Data;
            }
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
                            this.Add(new MediaOrderItem(newItem));
                        else
                            this.Insert(i, new MediaOrderItem(newItem));


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
    }
}
