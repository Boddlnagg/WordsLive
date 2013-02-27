using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Ionic.Zip;
using WordsLive.Utils.ImageLoader;

namespace WordsLive.Images
{
	public class ImageInfo : INotifyPropertyChanged
	{
		private object source;
		private SourceType sourceType;

		public Uri Uri
		{
			get
			{
				if (sourceType == SourceType.ZipFile)
					throw new InvalidOperationException("Loaded from a zip file.");

				return (Uri)source;
			}
		}

		public ZipEntry ZipEntry
		{
			get
			{
				if (sourceType != SourceType.ZipFile)
					throw new InvalidOperationException("Not loaded from a zip file.");

				return (ZipEntry)source;
			}
		}

		public object Source
		{
			get
			{
				return source;
			}
			private set
			{
				source = value;
				OnPropertyChanged("Source");
				OnPropertyChanged("Title");
			}
		}

		public string Title
		{
			get
			{
				switch (sourceType)
				{
					case SourceType.LocalDisk:
					case SourceType.ExternalResource:
						return Uri.Segments.Last();
					case SourceType.ZipFile:
						return ZipEntry.FileName;
					default:
						return null;
				}
			}
		}

		public bool IsLocalJpeg
		{
			get
			{
				if (SourceType != SourceType.LocalDisk)
					return false;

				var ext = new FileInfo(Uri.LocalPath).Extension.ToLower();
				return ext == ".jpg" || ext == ".jpeg";
			}
		}

		public SourceType SourceType
		{
			get
			{
				return sourceType;
			}
		}

		public ImageInfo(Uri uri)
		{
			if (uri.IsFile)
				this.sourceType = SourceType.LocalDisk;
			else
				this.sourceType = SourceType.ExternalResource;

			this.source = uri;
		}

		public ImageInfo(ZipEntry entry)
		{
			this.sourceType = SourceType.ZipFile;
			this.source = entry;
		}

		public void RotateLeft()
		{
			UpdateRotationMetadata(true);
			Source = new Uri(Uri, ""); // needed so ImageLoader reloads the source
		}

		public void RotateRight()
		{
			UpdateRotationMetadata(false);
			Source = new Uri(Uri, ""); // needed so ImageLoader reloads the source
		}

		private void UpdateRotationMetadata(bool rotateLeft)
		{
			if (!IsLocalJpeg)
				throw new InvalidOperationException("Can only rotate images loaded from local jpeg files.");

			using (Stream imageStream = new FileStream(Uri.LocalPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				BitmapFrame bitmapFrame = BitmapFrame.Create(imageStream);
				var metadataWriter = bitmapFrame.CreateInPlaceBitmapMetadataWriter();

				int rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);

				if (rotateLeft)
					rotation = (rotation + 270) % 360;
				else
					rotation = (rotation + 90) % 360;

				SetRotation(metadataWriter, rotation);

				metadataWriter.TrySave();

				rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);
			}
		}

		private int GetRotation(BitmapMetadata metadata)
		{
			int rotation = 0;

			if ((metadata != null) && metadata.ContainsQuery("System.Photo.Orientation"))
			{
				object o = metadata.GetQuery("System.Photo.Orientation");

				if (o != null)
				{
					//refer to http://www.impulseadventure.com/photo/exif-orientation.html for details on orientation values
					switch ((ushort)o)
					{
						case 6:
							rotation = 90;
							break;
						case 3:
							rotation = 180;
							break;
						case 8:
							rotation = 270;
							break;
					}
				}
			}

			return rotation;
		}

		private void SetRotation(BitmapMetadata metadata, int rotation)
		{
			if (metadata == null)
				throw new ArgumentNullException("metadata");

			int value = 1;

			switch(rotation)
			{
				case 0:
					value = 1; break;
				case 90:
					value = 6; break;
				case 180:
					value = 3; break;
				case 270:
					value = 8; break;
				default:
					throw new ArgumentException("rotation");
			}

			metadata.SetQuery("System.Photo.Orientation", value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
