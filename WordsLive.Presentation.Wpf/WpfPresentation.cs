using System;
using System.Windows;

namespace WordsLive.Presentation.Wpf
{
	/// <summary>
	/// Implements a presentation on top of a WPF FrameworkElement/Control.
	/// </summary>
	public class WpfPresentation<T> : IWpfPresentation where T : FrameworkElement,new()
	{
		private T control;
		private PresentationArea area;

		public WpfPresentation()
		{
			this.control = new T();
		}
		
		public T Control
		{
			get
			{
				return this.control;
			}
		}

		public PresentationArea Area
		{
			get
			{
				return WpfPresentationWindow.Instance.Area;   
			}
		}

		public virtual void Show(int transitionDuration = 0, Action callback = null, IPresentation previous = null)
		{
			WpfPresentationWindow.SetContent(this.Control, transitionDuration, callback, previous);
			WpfPresentationWindow.ShowWindow();
		}

		public virtual void TransitionTo(IPresentation target, int transitionDuration, Action callback)
		{
			WpfPresentationWindow.FadeOutContent(transitionDuration, callback);
			WpfPresentationWindow.ShowWindow();
		}
		
		public virtual void Hide()
		{
			WpfPresentationWindow.HideWindow();
		}
		
		private CloningWpfPreviewProvider<T> cachedPreview;
		
		public IPreviewProvider Preview
		{
			get
			{
				if (this.cachedPreview == null)
					this.cachedPreview = new CloningWpfPreviewProvider<T>(this.Control);
				
				return this.cachedPreview;
			}
		}


		public bool UsesSamePresentationWindow(IPresentation presentation)
		{
			return (presentation is IWpfPresentation);
		}


		public virtual void Close()
		{
			if (area != null)
			{
				area.WindowSizeChanged -= area_WindowSizeChanged;
			}
		}


		public bool TransitionPossibleFrom(IPresentation presentation)
		{
			return presentation != null;
		}

		public bool TransitionPossibleTo(IPresentation presentation)
		{
			return presentation != null;
		}

		public void Init(PresentationArea area)
		{
			WpfPresentationWindow.Instance.Area = area;
			this.area = area;

			this.control.Width = area.WindowSize.Width;
			this.control.Height = area.WindowSize.Height;

			area.WindowSizeChanged += area_WindowSizeChanged;
		}

		void area_WindowSizeChanged(object sender, EventArgs e)
		{
			this.control.Width = area.WindowSize.Width;
			this.control.Height = area.WindowSize.Height;
		}
	}
}
