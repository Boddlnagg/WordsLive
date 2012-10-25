using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
		/// <param name="file">The full path to a file.</param>
		public Media(string file)
		{
			LoadMetadata(file);
		}

		/// <summary>
		/// Loads just the metadata of this media object. The basic implementation just sets the file path.
		/// If you want to set a custom title, override this method.
		/// </summary>
		/// <param name="file">The file to load from.</param>
		protected virtual void LoadMetadata(string file)
		{
			File = file;
		}

		/// <summary>
		/// Reloads the metadata from the file by calling <see cref="LoadMetadata"/> again.
		/// To call this from outside this assembly use <see cref="MediaManager.ReloadMediaMetadata"/>.
		/// </summary>
		internal void ReloadMetadata()
		{
			LoadMetadata(File);
		}

		/// <summary>
		/// Gets the filename (full path) associated with this media object.
		/// </summary>
		public string File { get; private set; }

		/// <summary>
		/// Gets the title of this media object. The basic implementation just returns the file name.
		/// If you want to set a custom title, override this method.
		/// </summary>
		public virtual string Title
		{
			get
			{
				return string.IsNullOrEmpty(File) ? null : new FileInfo(File).Name;
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
