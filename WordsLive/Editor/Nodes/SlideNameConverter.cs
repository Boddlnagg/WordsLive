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
using System.Windows.Data;
using WordsLive.Core.Songs;

namespace WordsLive.Editor.Nodes
{
	/// <summary>
	/// Converts slides to slide names to be shown in the structure tree, numbering them from 1 to n in each part.
	/// </summary>
	public class SlideNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var slide = value as SongSlide;
			if (slide == null)
				return null;

			var part = slide.Root.FindPartWithSlide(slide);
			int i = part.Slides.IndexOf(slide);
			return String.Format("{0} {1}", WordsLive.Resources.Resource.eGridSlideTitle, (i + 1));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
