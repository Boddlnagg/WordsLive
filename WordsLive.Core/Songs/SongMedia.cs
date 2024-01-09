﻿/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

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

		public SongMedia(Uri uri, Dictionary<string, string> options) : base(uri, options) { }

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
			translationDisplayOptions = Options.ContainsKey("displayTranslation") && Enum.TryParse(Options["displayTranslation"], out TranslationDisplayOptions value) ? value : TranslationDisplayOptions.Default;
		}

		private TranslationDisplayOptions translationDisplayOptions;

		public TranslationDisplayOptions TranslationDisplayOptions
		{
			get
			{
				return translationDisplayOptions;
			}
			set
			{
				if (value != translationDisplayOptions)
				{
					translationDisplayOptions = value;
					if (translationDisplayOptions == TranslationDisplayOptions.Default)
					{
						Options.Remove("displayTranslation");
					}
					else
					{
						Options["displayTranslation"] = translationDisplayOptions.ToString();
					}
					OnOptionsChanged();
				}
			}
		}
	}

	/// <summary>
	/// Represents the display options of the song's text translation.
	/// </summary>
	public enum TranslationDisplayOptions
	{
		/// <summary>
		/// Show the song text in both its primary language and its translation (default).
		/// </summary>
		Default,

		/// <summary>
		/// Hide the translation, i.e., show only the song text in the primary language of the song file.
		/// </summary>
		Hide,

		/// <summary>
		/// Show only the translation of the song text.
		/// </summary>
		Only,

		/// <summary>
		/// Use translation of song text as primary language and use original text as its translation.
		/// </summary>
		Swap
	}
}
