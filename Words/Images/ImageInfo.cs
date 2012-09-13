using System.IO;
using Ionic.Zip;
using System.Windows.Media.Imaging;
using System;
using System.ComponentModel;

namespace Words.Images
{
	public class ImageInfo : INotifyPropertyChanged
	{
		private FileInfo file;

		public FileInfo File
		{
			get
			{
				return file;
			}
			private set
			{
				file = value;
				OnPropertyChanged("File");
				OnPropertyChanged("Source");
				OnPropertyChanged("Title");
			}
		}

		private ZipEntry zipEntry;

		public ZipEntry ZipEntry
		{
			get
			{
				return zipEntry;
			}
			private set
			{
				zipEntry = value;
				OnPropertyChanged("ZipEntry");
				OnPropertyChanged("Source");
				OnPropertyChanged("Title");
			}
		}

		public object Source
		{
			get
			{
				if (File != null)
					return File;
				else
					return ZipEntry;
			}
		}

		public string Title
		{
			get
			{
				if (File != null)
					return File.Name;
				else
					return ZipEntry.FileName;
			}
		}

		public bool IsJpeg
		{
			get
			{
				return this.File != null && (this.File.Extension.ToLower() == ".jpg" || this.File.Extension.ToLower() == ".jpeg");
			}
		}

		public Utils.ImageLoader.SourceType SourceType
		{
			get
			{
				if (File != null)
					return Utils.ImageLoader.SourceType.LocalDisk;
				else
					return Utils.ImageLoader.SourceType.ZipFile;
			}
		}

		public ImageInfo(FileInfo file)
		{
			this.File = file;
		}

		public ImageInfo(string filename)
		{
			this.File = new FileInfo(filename);
		}

		public ImageInfo(ZipEntry entry)
		{
			this.ZipEntry = entry;
		}

		public void RotateLeft()
		{
			UpdateRotationMetadata(true);
			this.File = new FileInfo(this.File.FullName);
		}

		public void RotateRight()
		{
			UpdateRotationMetadata(false);
			this.File = new FileInfo(this.File.FullName);
		}

		private void UpdateRotationMetadata(bool rotateLeft)
		{
			if (!IsJpeg)
				throw new InvalidOperationException("Can only rotate images loaded from jpeg files.");

			using (Stream imageStream = new FileStream(this.File.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
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
