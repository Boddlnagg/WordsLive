using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Words.Core;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows;

namespace Words.MediaOrderList
{
	public class MediaOrderItem : IActivatable, INotifyPropertyChanged
	{
		private IconProvider iconProvider;

		public MediaOrderItem(Media data)
		{
			this.Data = data;
			this.iconProvider = Controller.IconProviders.CreateProvider(data);
		}

		public Media Data { get; private set; }

		public void Reload()
		{
			this.Data.LoadMetadata(this.Data.File);
			this.iconProvider.Update();
			OnPropertyChanged("Title");
			OnPropertyChanged("Path");
			OnPropertyChanged("Icon");
		}

		public string Title
		{
			get
			{
				return Data.Title;
			}
		}

		public string Path
		{
			get
			{
				return Data.File;
			}
		}

		public ImageSource Icon
		{
			get
			{
				return this.iconProvider.Icon;
			}
		}
		
		public bool Activate()
		{
			// TODO (Words): refactor to get this logic out of MediaOrderItem ... somehow
			if (Data is UnsupportedMedia)
			{
				MessageBox.Show("Die Datei " + Data.File + " kann nicht angezeigt werden, da das Format nicht unterstützt wird.");
				return false;
			}
			else if (Data is FileNotFoundMedia)
			{
				MessageBox.Show("Die Datei " + Data.File + " existiert nicht.");
				return false;
			}
			else
			{
				return true;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
