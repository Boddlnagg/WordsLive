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

using System.Windows.Media;
using WordsLive.Core;

namespace WordsLive.MediaOrderList
{
	[TargetMedia(typeof(FileNotFoundMedia))]
	class FileNotFoundIconProvider : IconProvider
	{
		public FileNotFoundIconProvider(FileNotFoundMedia data) : base(data)
		{ }

		protected override ImageSource CreateIcon()
		{
			// TODO: create an image (or XAML-resource?)
			return null;
		}
	}
}
