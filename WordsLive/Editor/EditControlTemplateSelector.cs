/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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

using System.Windows;
using System.Windows.Controls;
using WordsLive.Core.Songs;

namespace WordsLive.Editor
{
	public class EditControlTemplateSelector : DataTemplateSelector
	{
		public DataTemplate EmptyTemplate { get; set; }
		public DataTemplate EditCopyrightTemplate { get; set; }
		public DataTemplate EditCategoryTemplate { get; set; }
		public DataTemplate EditTextWithTranslationTemplate { get; set; }
		public DataTemplate EditSourceTemplate { get; set; }
		public DataTemplate EditLanguageTemlate { get; set; }
		public DataTemplate EditCcliNumberTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			ISongElement element = (ISongElement)item;
			if (element is SongSlide)
				return EditTextWithTranslationTemplate;
			else if (element is Nodes.CategoryNode)
				return EditCategoryTemplate;
			else if (element is Nodes.CopyrightNode)
				return EditCopyrightTemplate;
			else if (element is Nodes.SourceNode)
				return EditSourceTemplate;
			else if (element is Nodes.LanguageNode)
				return EditLanguageTemlate;
			else if (element is Nodes.CcliNumberNode)
				return EditCcliNumberTemplate;
			else
				return EmptyTemplate;
		}
	}
}
