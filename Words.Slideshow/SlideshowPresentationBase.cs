using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Presentation;
using System.Windows.Media;

namespace Words.Slideshow
{
	public abstract class SlideshowPresentationBase : ISlideshowPresentation
	{
		private SlideshowPreviewProvider preview;

		public abstract void Load();

		public virtual bool UsesSamePresentationWindow(IPresentation presentation)
		{
			return false;
		}

		public virtual bool TransitionPossibleFrom(IPresentation presentation)
		{
			return false;
		}

		public virtual void Init(PresentationArea area)
		{
			this.area = area;
		}

		private PresentationArea area;

		public PresentationArea Area
		{
			get
			{
				return area;
			}
		}

		public virtual void Show(int transitionDuration = 0, Action callback = null)
		{
			// NOTE: transition duration will be ignored (not possible with external presentations)
			Show();
			if (callback != null)
				callback();
		}

		protected void LoadPreviewProvider()
		{
			preview = new SlideshowPreviewProvider(this);
		}

		public IPreviewProvider Preview
		{
			get
			{
				return preview;
			}
		}

		#region Events
		
		public event EventHandler SlideIndexChanged;

		protected void OnSlideIndexChanged()
		{
			if (SlideIndexChanged != null)
				SlideIndexChanged(this, EventArgs.Empty);
		}

		public event EventHandler Loaded;

		protected void OnLoaded()
		{
			if (Loaded != null)
				Loaded(this, EventArgs.Empty);
		}

		public event EventHandler ClosedExternally;

		protected void OnClosedExternally()
		{
			if (ClosedExternally != null)
				ClosedExternally(this, EventArgs.Empty);
		}

		#endregion

		#region Abstract members

		public abstract void GotoSlide(int index);

		public abstract void NextStep();

		public abstract void PreviousStep();

		public abstract void Show();

		public abstract void Close();

		public abstract void Hide();

		public abstract IList<ImageSource> Thumbnails { get; }

		public abstract int SlideIndex { get; }

		#endregion
	}
}
