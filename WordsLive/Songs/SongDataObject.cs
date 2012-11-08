using System;
using System.Windows;
using WordsLive.Core.Data;

namespace WordsLive.Songs
{
	/// <summary>
	/// An implementation of <see cref="IDataObject"/> that is used for drag & drop actions from the song list
	/// and automatically downloads songs on demand using the song provider's GetLocal() method.
	/// </summary>
	public class SongDataObject : IDataObject
	{
		private SongData data;

		public static readonly string SongDataFormat = "SongData";

		public SongDataObject(SongData data)
		{
			this.data = data;
		}

		public object GetData(string format, bool autoConvert)
		{
			if (format == DataFormats.FileDrop && autoConvert)
			{
				try
				{
					return new string[] { DataManager.Songs.GetLocal(data.Filename).FullName };
				}
				catch
				{
					return null;
				}
			}
			else if (format == SongDataFormat)
			{
				return data;
			}
			else
			{
				return null;
			}
		}

		public object GetData(Type format)
		{
			if (format == typeof(SongData))
				return data;
			else
				return null;
		}

		public object GetData(string format)
		{
			if (format == SongDataFormat)
				return data;
			else
				return null;
		}

		public bool GetDataPresent(string format, bool autoConvert)
		{
			return (format == DataFormats.FileDrop && autoConvert) || format == SongDataFormat;
		}

		public bool GetDataPresent(Type format)
		{
			return format == typeof(SongData);

		}

		public bool GetDataPresent(string format)
		{
			return format == SongDataFormat;
		}

		public string[] GetFormats(bool autoConvert)
		{
			if (autoConvert)
				return new string[] { SongDataFormat, DataFormats.FileDrop };
			else
				return new string[] { SongDataFormat };
		}

		public string[] GetFormats()
		{
			return new string[] { SongDataFormat };
		}

		public void SetData(string format, object data, bool autoConvert)
		{
			throw new NotSupportedException();
		}

		public void SetData(Type format, object data)
		{
			throw new NotSupportedException();
		}

		public void SetData(string format, object data)
		{
			throw new NotSupportedException();
		}

		public void SetData(object data)
		{
			throw new NotSupportedException();
		}
	}
}
