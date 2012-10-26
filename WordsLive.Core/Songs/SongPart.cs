/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using System.Linq;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a part of a song.
	/// </summary>
	public class SongPart
	{
		/// <summary>
		/// Gets or sets the name of the part. Must be unique in a song.
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets a list of slides.
		/// </summary>
		public List<SongSlide> Slides { get; set; }

		/// <summary>
		/// Gets the text of all slides in this part.
		/// </summary>
		public string Text
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.Text).ToArray());
			}
		}

		/// <summary>
		/// Gets the text of all slides in this part, but with chords removed.
		/// </summary>
		public string TextWithoutChords
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.TextWithoutChords).ToArray());
			}
		}

		/// <summary>
		/// Gets a value indicating whether any slide in this part has a translation.
		/// </summary>
		public bool HasTranslation
		{
			get
			{
				foreach (SongSlide slide in this.Slides)
				{
					if (!String.IsNullOrEmpty(slide.Translation))
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPart"/> class.
		/// </summary>
		/// <param name="name">The part's name.</param>
		public SongPart(string name)
		{
			this.Name = name;
			this.Slides = new List<SongSlide>();
		}
	}
}
