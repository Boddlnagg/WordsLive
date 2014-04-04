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

using System;
using System.Collections.Generic;
using WordsLive.Presentation;

namespace WordsLive.Slideshow.Presentation
{
	public class SlideshowLoadedEventArgs : EventArgs
	{
		public bool Success { get; private set; }

		public SlideshowLoadedEventArgs(bool success)
		{
			Success = success;
		}
	}

	public interface ISlideshowPresentation : IPresentation
	{
		void Load();
		void GotoSlide(int index);
		void NextStep();
		void PreviousStep();
		int SlideIndex { get; }
		IList<SlideThumbnail> Thumbnails { get; }
		bool IsEndless { get; }
		event EventHandler<SlideshowLoadedEventArgs> Loaded;
		event EventHandler SlideIndexChanged;
		event EventHandler ClosedExternally;
	}
}
