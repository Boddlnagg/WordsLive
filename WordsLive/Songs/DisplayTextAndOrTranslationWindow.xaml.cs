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
using WordsLive.Core.Songs;

namespace WordsLive.Songs
{
	public partial class DisplayTextAndOrTranslationWindow : Window
	{
		public DisplayTextAndOrTranslationWindow(SongMedia songMedia, string language, string translationLanguage)
		{
			InitializeComponent();
			DataContext = songMedia;

			language = string.IsNullOrEmpty(language) ? "?" : language;
			translationLanguage = string.IsNullOrEmpty(translationLanguage) ? "?" : translationLanguage;

			PrimaryLanguageForTextAndTranslation.Content = language;
			SecondaryLanguageForTextAndTranslation.Content = translationLanguage;
			PrimaryLanguageForText.Content = language;
			PrimaryLanguageForTranslation.Content = translationLanguage;
			PrimaryLanguageForTranslationAndText.Content = translationLanguage;
			SecondaryLanguageForTranslationAndText.Content = language;
		}
	}
}
