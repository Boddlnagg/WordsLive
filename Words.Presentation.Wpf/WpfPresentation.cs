using System;
using Words.Presentation;
using System.Windows.Controls;
using System.Windows;

namespace Words.Presentation.Wpf
{
	/// <summary>
	/// Implements a presentation on top of a WPF FrameworkElement/Control.
	/// </summary>
	public class WpfPresentation<T> : IWpfPresentation where T : FrameworkElement,new()
	{
		private T control;

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

		public virtual void Show(int transitionDuration = 0, Action callback = null)
		{
			WpfPresentationWindow.SetContent(this.Control, transitionDuration, callback);
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
			// does nothing, because the WpfPresentationWindow needs to be kept alive
		}


		public bool TransitionPossibleFrom(IPresentation presentation)
		{
			return (presentation is IWpfPresentation);
		}

		public void Init(PresentationArea area)
		{
			WpfPresentationWindow.Instance.Area = area;
		}
	}
}
