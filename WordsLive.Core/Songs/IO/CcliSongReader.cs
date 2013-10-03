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

using System.IO;
namespace WordsLive.Core.Songs.IO
{
	public abstract class CcliSongReader : ISongReader
	{
		protected void SetSourceFromCcliNumber(Song song, int num)
		{
			var source = new SongSource(song);
			source.Songbook = "CCLI SongSelect";
			source.Number = num;
			song.Sources.Clear();
			song.Sources.Add(source);
		}

		protected string GetPartName(string line, out bool checkFirstLine)
		{
			checkFirstLine = false;

			if (!(line.StartsWith("Ver") || line.StartsWith("Ch") || line.StartsWith("Br")))
			{
				checkFirstLine = true;
			}

			return line;
		}

		protected bool CheckFirstLine(string line, ref string partName)
		{
			if (line.StartsWith("(") && line.EndsWith(")"))
			{
				partName = line.Substring(1, line.Length - 2);
				return true;
			}
			else
			{
				return false;
			}
		}

		public abstract void Read(Song song, Stream stream);
	}
}
