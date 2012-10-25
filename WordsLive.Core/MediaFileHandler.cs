using System.Collections.Generic;
using System.IO;

namespace WordsLive.Core
{
	/// <summary>
	/// Abstract base class for media file handlers. Subclass this to implement a new media file type.
	/// Subclasses found in extension assemblies are automatically registered as handlers.
	/// </summary>
	public abstract class MediaFileHandler
	{
		/// <summary>
		/// The allowed file extensions for this media file type (with leading '.').
		/// </summary>
		public abstract IEnumerable<string> Extensions { get; }

		/// <summary>
		/// A description of this media type. (TODO: localize)
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Try to load a single file with this handler.
		/// </summary>
		/// <param name="file">The file to load.</param>
		/// <returns>The loaded media object.</returns>
		public abstract Media TryHandle(FileInfo file);

		/// <summary>
		/// Try to load multiple files at once. If this is not supported by the media type or some or all files are not supported,
		/// <c>null</c> is returned.
		/// </summary>
		/// <param name="files">The files to load.</param>
		/// <returns>The loaded media objects or <c>null</c>.</returns>
		public virtual IEnumerable<Media> TryHandleMultiple(IEnumerable<FileInfo> files)
		{
			return null;
		}
	}
}
