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

using System;
using WordsLive.Presentation;

namespace WordsLive.Documents
{
	public interface IDocumentPresentation : IPresentation
	{
		void Load();
		bool IsLoaded { get; }
		int PageCount { get; }
		int CurrentPage { get; }
		void GoToPage(int page);
		void PreviousPage();
		void NextPage();
		bool CanGoToPreviousPage { get; }
		bool CanGoToNextPage { get; }
		DocumentPageScale PageScale { get; set; }
		event EventHandler DocumentLoaded;
	}
}
