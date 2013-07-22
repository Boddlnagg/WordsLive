using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using WordsLive.Core;

namespace WordsLive.MediaOrderList
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
			else
			{
				iconProvider.Invalidate();
			}
			OnPropertyChanged("Title");
			OnPropertyChanged("Path");
			OnPropertyChanged("Icon");
			OnPropertyChanged("IsActivatable");
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
				if (Data.Uri.IsFile)
				{
					return Uri.UnescapeDataString(Data.Uri.Segments.Last());
				}
				else if (Data.Uri.Scheme == "song")
				{
					return Uri.UnescapeDataString(Data.Uri.AbsolutePath).Substring(1);
				}
				else
				{
					return Uri.UnescapeDataString(Data.Uri.AbsoluteUri);
				}
			}
		}

		public ImageSource Icon
		{
			get
			{
				return this.iconProvider.Icon;
			}
		}

		public bool IsActivatable
		{
			get
			{
				return !(this.Data is FileNotFoundMedia || this.Data is UnsupportedMedia);
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
