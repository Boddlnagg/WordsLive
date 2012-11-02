using System.IO;
using WordsLive.Core.Data;

namespace WordsLive.Core
{
	/// <summary>
	/// Abstract base class for all media types.
	/// </summary>
	public abstract class Media
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Media"/> class by loading the metadata
		/// from the given file.
		/// </summary>
		/// <param name="file">The relative path to the resource.</param>
		public Media(string file, MediaDataProvider provider)
		{
			File = file;
			DataProvider = provider;

			LoadMetadata();
		}

		/// <summary>
		/// Loads just the metadata of this media object. This is called again to
		/// reload in <see cref="MediaManager.ReloadMediaMetadata"/>
		/// </summary>
		internal virtual void LoadMetadata()
		{
			// do nothing
		}

		/// <summary>
		/// Gets the filename (relative or absolute) associated with this media object.
		/// </summary>
		public string File { get; private set; }

		/// <summary>
		/// Gets the data provider used for this media instance.
		/// </summary>
		public MediaDataProvider DataProvider { get; private set; }

		/// <summary>
		/// Gets the title of this media object. The basic implementation just returns the file name.
		/// If you want to set a custom title, override this method.
		/// </summary>
		public virtual string Title
		{
			get
			{
				return string.IsNullOrEmpty(File) ? null : Path.GetFileName(File);
			}
		}

		/// <summary>
		/// Loads the media object from the file specified in the <see cref="File"/> field into memory.
		/// This is always called before the control panel and/or presentation is shown.
		/// Use <see cref="MediaManager.LoadMedia"/> to call this safely.
		/// </summary>
		public abstract void Load();
	}
}
