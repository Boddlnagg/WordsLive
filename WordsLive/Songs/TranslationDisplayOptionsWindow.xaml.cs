﻿/*
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
	public partial class TranslationDisplayOptionsWindow : Window
	{
		public TranslationDisplayOptionsWindow(SongMedia songMedia, string language, string translationLanguage)
		{
			InitializeComponent();
			DataContext = songMedia;

			(FindName("PrimaryLanguageForDefault") as Label).Content = language;
			(FindName("SecondaryLanguageForDefault") as Label).Content = translationLanguage;
			(FindName("PrimaryLanguageForHide") as Label).Content = language;
			(FindName("PrimaryLanguageForOnly") as Label).Content = translationLanguage;
			(FindName("PrimaryLanguageForSwap") as Label).Content = translationLanguage;
			(FindName("SecondaryLanguageForSwap") as Label).Content = language;
		}
	}
}