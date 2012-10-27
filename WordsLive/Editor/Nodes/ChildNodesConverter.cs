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
using System.Windows.Data;
using WordsLive.Core.Songs;

namespace WordsLive.Editor.Nodes
{
	/// <summary>
	/// Converter to collect the child nodes for a song, including the metadata nodes.
	/// This is used in the song structure tree.
	/// </summary>
	public class ChildNodesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Song song = value as Song;
			if (song == null)
				return null;

			List<object> children = new List<object>();
			
			// add parts
			foreach (var part in song.Parts)
			{
				children.Add(part);
			}

			// add metadata nodes
			children.Add(new SourceNode(song));
			children.Add(new CopyrightNode(song));
			children.Add(new LanguageNode(song));
			children.Add(new CategoryNode(song));

			return children;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
