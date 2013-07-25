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
using System.Windows.Controls;

namespace WordsLive.AudioVideo
{
	public abstract class BaseMediaControl : UserControl
	{
		public abstract TimeSpan Duration { get; }
		public abstract int Volume { get; set; }
		public abstract int Position { get; set; }
		public abstract bool Loop { get; set; }
		public abstract bool Autoplay { get; set; }
		public abstract void Load(Uri uri);
		public abstract void Play();
		public abstract void Pause();
		public abstract void Stop();
		public abstract void Destroy();

		public abstract event Action MediaLoaded;
		public abstract event Action MediaFailed;
		public abstract event Action PlaybackEnded;
		public abstract event Action SeekStart;
	}
}
