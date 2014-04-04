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
using unoidl.com.sun.star.animations;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.presentation;

namespace WordsLive.Slideshow.Impress.Bridge
{
	/// <summary>
	/// This implements an UNO interface and maps handlers to C# events.
	/// </summary>
	public class SlideShowListener : XSlideShowListener
	{
		public event EventHandler SlideTransitionStarted;
		public event EventHandler SlideTransitionEnded;
		public event EventHandler SlideEnded;
		public event EventHandler Paused;
		public event EventHandler SlideAnimationsEnded;

		public void slideTransitionStarted()
		{
			if (SlideTransitionStarted != null)
				SlideTransitionStarted(this, EventArgs.Empty);
		}

		public void slideTransitionEnded()
		{
			if (SlideTransitionEnded != null)
				SlideTransitionEnded(this, EventArgs.Empty);
		}

		public void slideEnded(bool reverse)
		{
			if (SlideEnded != null)
				SlideEnded(this, EventArgs.Empty);
		}

		public void paused()
		{
			if (Paused != null)
				Paused(this, EventArgs.Empty);
		}

		public void beginEvent(XAnimationNode Node) { }
		public void endEvent(XAnimationNode Node) { }
		public void repeat(XAnimationNode Node, int Repeat) { }
		public void disposing(EventObject Source) { }
		public void hyperLinkClicked(string hyperLink) { }
		public void resumed() { }
		public void slideAnimationsEnded()
		{
			if (SlideAnimationsEnded != null)
				SlideAnimationsEnded(this, EventArgs.Empty);
		}
	}
}
