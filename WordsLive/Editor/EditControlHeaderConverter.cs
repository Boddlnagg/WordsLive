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

namespace WordsLive.Editor
{
	/// <summary>
	/// Converter to get the header of the edit control depending on the element
	/// that is currently selected/edited.
	/// </summary>
	public class EditControlHeaderConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var element = (ISongElement)value;

			if (element is SongSlide)
			{
				return WordsLive.Resources.Resource.eGridTextHeader;
			}
			else if (element is Nodes.LanguageNode)
			{
				return WordsLive.Resources.Resource.eMetadataLanguageTitle;
			}
			else if (element is Nodes.SourceNode)
			{
				return WordsLive.Resources.Resource.eMetadataSourceTitle;
			}
			else if (element is Nodes.CopyrightNode)
			{
				return WordsLive.Resources.Resource.eMetadataCopyrightTitle;
			}
			else if (element is Nodes.CategoryNode)
			{
				return WordsLive.Resources.Resource.eMetadataCategoryTitle;
			}
			else
			{
				return String.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
