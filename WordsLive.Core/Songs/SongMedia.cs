/*
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
		private static readonly string DisplayTextAndOrTranslationKey = "displayTextAndOrTranslation";

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
			displayTextAndOrTranslation = Options.ContainsKey(DisplayTextAndOrTranslationKey) && Enum.TryParse(Options[DisplayTextAndOrTranslationKey], out DisplayTextAndOrTranslation value) ? value : DisplayTextAndOrTranslation.TextAndTranslation;
		}

		private DisplayTextAndOrTranslation displayTextAndOrTranslation;

		public DisplayTextAndOrTranslation DisplayTextAndOrTranslation
		{
			get
			{
				return displayTextAndOrTranslation;
			}
			set
			{
				if (value != displayTextAndOrTranslation)
				{
					displayTextAndOrTranslation = value;
					if (displayTextAndOrTranslation == DisplayTextAndOrTranslation.TextAndTranslation)
					{
						Options.Remove(DisplayTextAndOrTranslationKey);
					}
					else
					{
						Options[DisplayTextAndOrTranslationKey] = displayTextAndOrTranslation.ToString();
					}
					OnOptionsChanged();
				}
			}
		}
	}

	/// <summary>
	/// Represents the display options of the song's text and its translation.
	/// </summary>
	public enum DisplayTextAndOrTranslation
	{
		/// <summary>
		/// Show the text and its translation (default).
		/// </summary>
		TextAndTranslation,

		/// <summary>
		/// Show the text but without its translation.
		/// </summary>
		Text,

		/// <summary>
		/// Show the translation but not the original text.
		/// </summary>
		Translation,

		/// <summary>
		/// Show translation and text (i.e., they are swapped).
		/// </summary>
		TranslationAndText
	}
}
