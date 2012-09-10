using System;
using System.ComponentModel;
using System.Windows.Media;
using Words.Core;
using Words.Utils.ActivatableListBox;

namespace Words.MediaOrderList
{
	public class MediaOrderItem : INotifyPropertyChanged
	{
		private IconProvider iconProvider;
		private Func<Media, bool> activationHandler;

		public MediaOrderItem(Media data, Func<Media, bool> activationHandler)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			this.Data = data;

			if (activationHandler == null)
				throw new ArgumentNullException("activationHandler");
			this.activationHandler = activationHandler;

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
			return activationHandler(Data);
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
