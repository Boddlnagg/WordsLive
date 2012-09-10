using System;
using System.ComponentModel;
using System.Windows.Media;
using Words.Core;
using Words.Utils.ActivatableListBox;
using System.IO;

namespace Words.MediaOrderList
{
	public class MediaOrderItem : INotifyPropertyChanged
	{
		private IconProvider iconProvider;

		public MediaOrderItem(Media data)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			this.Data = data;

			this.iconProvider = Controller.IconProviders.CreateProvider(data);
		}

		public Media Data { get; private set; }

		public void ReloadMetadata()
		{
			var newData = MediaManager.ReloadMediaMetadata(Data);
			if (newData != Data)
			{
				Data = newData;
				iconProvider = Controller.IconProviders.CreateProvider(newData);
			}
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
