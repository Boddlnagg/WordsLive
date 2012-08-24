using System;
using unoidl.com.sun.star.presentation;
using unoidl.com.sun.star.lang;
using unoidl.com.sun.star.animations;

namespace Words.Slideshow.Impress.Bridge
{
	/// <summary>
	/// This implements an UNO interface and maps handlers to C# events.
	/// </summary>
	public class SlideShowListener : XSlideShowListener
	{
		public event EventHandler SlideTransitionStarted;
		public event EventHandler SlideTransitionEnded;
		public event EventHandler SlideEnded;

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

		public void beginEvent(XAnimationNode Node) { }
		public void endEvent(XAnimationNode Node) { }
		public void repeat(XAnimationNode Node, int Repeat) { }
		public void disposing(EventObject Source) { }
		public void hyperLinkClicked(string hyperLink) { }
		public void paused() { }
		public void resumed() { }
		public void slideAnimationsEnded() { }
	}
}
