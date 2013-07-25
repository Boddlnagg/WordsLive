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
