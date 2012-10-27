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

using System.Drawing;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a song background. This is either an image or a color.
	/// TODO: support video backgrounds
	/// </summary>
	public class SongBackground
	{
		/// <summary>
		/// Gets a value indicating whether this background is an image.
		/// </summary>
		public bool IsImage
		{
			get
			{
				return (!string.IsNullOrEmpty(ImagePath));
			}
		}

		/// <summary>
		/// Gets or sets the image path. Set this to <c>null</c> to use a color instead.
		/// </summary>
		public string ImagePath { get; set; }

		/// <summary>
		/// Gets or sets the color (only valid if <see cref="IsImage"/> is <c>false</c>).
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SongBackground"/> class
		/// using a black background color.
		/// </summary>
		public SongBackground()
		{
			ImagePath = "";
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
			SongBackground bg = obj as SongBackground;
			if (bg == null)
				return false;

			if (this.IsImage)
			{
				return bg.IsImage && bg.ImagePath == this.ImagePath;
			}
			else
			{
				return !bg.IsImage && bg.Color.ToArgb() == this.Color.ToArgb();
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
			if (this.IsImage)
				return IsImage.GetHashCode() ^ this.ImagePath.GetHashCode();
			else
				return IsImage.GetHashCode() ^ this.Color.GetHashCode();
		}
	}
}