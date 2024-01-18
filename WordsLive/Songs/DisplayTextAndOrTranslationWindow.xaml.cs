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

namespace WordsLive.Songs
{
	public partial class DisplayTextAndOrTranslationWindow : Window
	{
		public DisplayTextAndOrTranslationWindow(SongMedia songMedia, string language, string translationLanguage)
		{
			InitializeComponent();
			DataContext = songMedia;

			(FindName("PrimaryLanguageForTextAndTranslation") as Label).Content = language;
			(FindName("SecondaryLanguageForTextAndTranslation") as Label).Content = translationLanguage;
			(FindName("PrimaryLanguageForText") as Label).Content = language;
			(FindName("PrimaryLanguageForTranslation") as Label).Content = translationLanguage;
			(FindName("PrimaryLanguageForTranslationAndText") as Label).Content = translationLanguage;
			(FindName("SecondaryLanguageForTranslationAndText") as Label).Content = language;
		}
	}
}
