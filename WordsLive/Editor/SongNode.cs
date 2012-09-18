using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MonitoredUndo;

namespace WordsLive.Editor
{
	public abstract class SongNode : INotifyPropertyChanged, ISupportsUndo
	{
		private SongNodeRoot root;

		public SongNode(SongNodeRoot root)
		{
			if (this is SongNodeRoot)
				this.root = (SongNodeRoot)this;
			else
				this.root = root;
		}

		public SongNodeRoot Root
		{
			get
			{
				return this.root;
			}
		}

		private string title;
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				DefaultChangeFactory.OnChanging(this, "Title", this.title, value);
				this.title = value;
				OnNotifyPropertyChanged("Title");
			}
		}

		public abstract string IconUri { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public object GetUndoRoot()
		{
			return this.Root;
		}
	}
}
