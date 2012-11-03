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
using System.IO;

namespace WordsLive.Core.Data
{
	public class LocalFileDataProvider : MediaDataProvider
	{
		public override Stream Get(string path)
		{
			return File.OpenRead(path);
		}

		public override Uri GetUri(string path)
		{
			if (!File.Exists(path))
				throw new FileNotFoundException(path);

			return new Uri(path);
		}

		public override FileInfo GetLocal(string path)
		{
			var fi = new FileInfo(path);

			if (!fi.Exists)
				throw new FileNotFoundException(path);

			return fi;
		}
	}
}
