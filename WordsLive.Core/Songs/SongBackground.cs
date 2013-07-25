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
using System.Drawing;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a song background (either an image or a color).
	/// This class is immutable.
	/// </summary>
	[JsonConverter(typeof(Json.JsonSongBackgroundConverter))]
	public class SongBackground
	{
		/// <summary>
		/// Gets the type of this background.
		/// </summary>
		public SongBackgroundType Type { get; private set; }

		/// <summary>
		/// The default background (black).
		/// </summary>
		public static readonly SongBackground Default = new SongBackground(Color.Black);

		/// <summary>
		/// Gets the image or video path. (only valid if <see cref="Type"/> is Image or Video).
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// Gets or sets the color (only valid if <see cref="Type"/> is Color).
		/// </summary>
		public Color Color { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this background is based on a file (image or video).
		/// </summary>
		public bool IsFile
		{
			get
			{
				return Type == SongBackgroundType.Image || Type == SongBackgroundType.Video;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongBackground"/> class
		/// using a black background color.
		/// </summary>
		public SongBackground(Color color)
		{
			FilePath = null;
			Color = color;
			Type = SongBackgroundType.Color;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongBackground"/> class.
		/// </summary>
		/// <param name="filePath">The file path (image or video).</param>
		/// <param name="isVideo">if set to <c>true</c> the backgound is a video background.</param>
		public SongBackground(string filePath, bool isVideo)
		{
			if (String.IsNullOrEmpty(filePath))
				throw new ArgumentException("filePath");

			FilePath = filePath;
			Type = isVideo ? SongBackgroundType.Video : SongBackgroundType.Image;
			Color = Color.Black;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SongBackground))
				return false;

			var bg = (SongBackground)obj;

			if (this.IsFile)
			{
				return bg.Type == this.Type && bg.FilePath == this.FilePath;
			}
			else
			{
				return bg.Type == this.Type && bg.Color.ToArgb() == this.Color.ToArgb();
			}
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			if (this.IsFile)
				return Type.GetHashCode() ^ this.FilePath.GetHashCode();
			else
				return Type.GetHashCode() ^ this.Color.GetHashCode();
		}
	}
}