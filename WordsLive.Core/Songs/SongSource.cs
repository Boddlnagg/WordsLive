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

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents the source information of a song.
	/// </summary>
	public class SongSource
	{
		/// <summary>
		/// Gets or sets the songbook.
		/// </summary>
		public string Songbook { get; set;}

		/// <summary>
		/// Gets or sets the number of the song in the songbook (0 if unknown)
		/// </summary>
		public int Number { get; set;}
		
		/// <summary>
		/// Generated a <see cref="SongSource"/> by parsing a string.
		/// A variety of formats is supported, e.g. '[Book] / [Nr]' or '[Book], Nr. [Nr]'.
		/// </summary>
		/// <param name="source">The string to parse.</param>
		/// <returns>The parsed source object.</returns>
		public static SongSource Parse(string source)
		{
			SongSource result = new SongSource();
			
			if(source.Trim() == "")
				return result;
			
			bool success = false;
			int n;
			
			if (source.Contains("/"))
			{
				var parts = source.Split('/');
				result.Songbook = parts[0].Trim();
				if (int.TryParse(parts[1].Trim(), out n))
				{
					result.Number = n;
					success = true;
				}
			}
	
			if (!success)
			{
				int index = source.LastIndexOfAny(new char[] {' ','.',','});
				if (index >= 0 && int.TryParse(source.Substring(index+1).Trim(), out n))
				{
					result.Number = n;
					string book = source.Substring(0, index+1).Trim();
					if (book.EndsWith("Nr."))
						book = book.Substring(0, book.Length - 3).Trim();
					if (book.EndsWith(","))
						book = book.Substring(0, book.Length - 1).Trim();
					result.Songbook = book;
				}
				else
				{
					result.Songbook = source;
				}
			}
			
			return result;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance in the format '[Book] / [Nr]'.
		/// If the <see cref="Number"/> is <c>0</c> only the <see cref="Songbook"/> name is returned.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(Songbook))
				return string.Empty;
			else if (Number == 0)
				return Songbook;
			else
				return Songbook + " / " + Number;
		}
	}
}