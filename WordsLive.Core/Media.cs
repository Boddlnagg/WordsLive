using System;
using System.Linq;
using Newtonsoft.Json;
using WordsLive.Core.Data;

namespace WordsLive.Core
{
	/// <summary>
	/// Abstract base class for all media types.
	/// </summary>
	public abstract class Media
	{
		/// <summary>
		/// Gets the URI associated with this media object.
		/// <c>null</c> if this media object has not been loaded from a URI and has not been saved yet.
		/// </summary>
		public Uri Uri { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Media"/> class by loading the metadata
		/// from the given URI.
		/// </summary>
		/// <param name="uri">The URI pointing to the media resource.</param>
		public Media(Uri uri)
		{
			Uri = uri;
		}

		/// <summary>
		/// Loads just the metadata of this media object.
		/// </summary>
		protected virtual void LoadMetadata()
		{
			// do nothing
		}

		/// <summary>
		/// Internal interface to LoadMetadata(). This is called in  <see cref="MediaManager.ReloadMediaMetadata"/>.
		/// </summary>
		internal void LoadMetadataHelper()
		{
			LoadMetadata();
		}

		/// <summary>
		/// Gets the data provider used for this media instance.
		/// </summary>
		[JsonIgnore]
		public IMediaDataProvider DataProvider
		{
			get
			{
				return null; // TODO!!
			}
		}

		public string File
		{
			get
			{
				return null; // TODO!!
			}
		}

		/// <summary>
		/// Gets the title of this media object. The basic implementation just returns the file name.
		/// If you want to set a custom title, override this method.
		/// </summary>
		public virtual string Title
		{
			get
			{
				return Uri == null ? null : Uri.UnescapeDataString(Uri.Segments.Last());
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
