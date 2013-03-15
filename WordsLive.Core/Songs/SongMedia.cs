using System;

namespace WordsLive.Core.Songs
{
	public class SongMedia : Media
	{
		public Song Song { get; private set; }

		/// <summary>
		/// Gets the title of this media object.
		/// </summary>
		public override string Title
		{
			get
			{
				return Song.Title;
			}
		}

		public SongMedia(Uri uri) : base(uri) { }

		/// <summary>
		/// Load the song in order to have access to the title and background.
		/// </summary>
		/// <param name="filename">The file to load.</param>
		protected override void LoadMetadata()
		{
			base.LoadMetadata();
			Load();
		}

		/// <summary>
		/// Loads the media object from the file specified in the <see cref="File"/> field into memory.
		/// This is always called before the control panel and/or presentation is shown.
		/// Use <see cref="MediaManager.LoadMedia"/> to call this safely.
		/// </summary>
		public override void Load()
		{
			Song = new Song(this.Uri);
		}
	}
}
