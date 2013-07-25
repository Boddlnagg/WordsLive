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
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace WordsLive.Presentation.Wpf
{
	public partial class WpfPresentationWindow : Window
	{ 
		private static WpfPresentationWindow instance;
		public static WpfPresentationWindow Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new WpfPresentationWindow();
				}
				return instance;
			}
		}

		private PresentationArea area;

		private WpfPresentationWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Sets another control as the new content and starts a transition.
		/// </summary>
		/// <param name="control">The control to show next.</param>
		/// <param name="milliseconds">The duration of the transition in milliseconds.</param>
		public static void SetContent(FrameworkElement control, int milliseconds, Action callback, IPresentation previous)
		{
			bool transitionFromNonWpf = previous != null && !(previous is IWpfPresentation);

			if (Instance.MainContainer.Child == control && !transitionFromNonWpf)
				return;

			if (contentTransitionCallback != null)
				contentTransitionCallback();

			contentTransitionCallback = callback;

			Instance.PreviousContainer.Child = null;
			UIElement previousControl;
			if (transitionFromNonWpf)
				previousControl = null;
			else
				previousControl = Instance.MainContainer.Child;
			Instance.MainContainer.Child = control;
			Instance.PreviousContainer.Child = previousControl;
			Instance.PreviousContainer.Opacity = 1;
			Instance.MainContainer.Opacity = 0;

			Storyboard sbd = (Storyboard)Instance.FindResource("ContentTransition");
			sbd.Children[0].Duration = new TimeSpan(0, 0, 0, 0, milliseconds);
			sbd.Begin(Instance);
		}

		public static void FadeOutContent(int milliseconds, Action callback)
		{
			if (contentTransitionCallback != null)
				contentTransitionCallback();

			contentTransitionCallback = callback;

			// if the current control is blackscreen, then remove the previous control first
			// (because it has not been removed in ContentTransition_Completed)
			if (Instance.MainContainer.Child is BlackscreenControl)
			{
				Instance.PreviousContainer.Child = null;
				Instance.PreviousContainer.Opacity = 1;
			}

			Storyboard sbd = (Storyboard)Instance.FindResource("ContentFadeOut");
			sbd.Children[0].Duration = new TimeSpan(0, 0, 0, 0, milliseconds);
			sbd.Begin(Instance);
		}
		
		private static Action contentTransitionCallback = null;

		private void ContentTransition_Completed(object sender, EventArgs e)
		{
			// temporarily keep the previous control if the current one is blackscreen
			// (because the previous control is probably still shown in the preview area)
			if (!(Instance.MainContainer.Child is BlackscreenControl))
			{
				Instance.PreviousContainer.Child = null;
				Instance.PreviousContainer.Opacity = 1;
			}

			if (contentTransitionCallback != null)
			{
				Action callback = contentTransitionCallback;
				contentTransitionCallback = null;
				callback();
			}

			Instance.MainContainer.Opacity = 1;
			
		}

		public PresentationArea Area
		{
			get
			{
				return this.area;
			}
			set
			{
				if (this.area != value)
				{
					this.area = value;
					this.Left = this.area.WindowLocation.X;
					this.Top = this.area.WindowLocation.Y;
					this.Width = this.area.WindowSize.Width;
					this.Height = this.area.WindowSize.Height;

					this.area.WindowSizeChanged += delegate
					{
						this.Width = this.area.WindowSize.Width;
						this.Height = this.area.WindowSize.Height;
					};

					this.area.WindowLocationChanged += delegate
					{
						//if (hidden == false)
						//{
							this.Left = this.area.WindowLocation.X;
							this.Top = this.area.WindowLocation.Y;
						//}
					};
				}
			}
		}

		private static bool? hidden = null;

		private void InitWindow()
		{
			// the window has not been shown before, so show it
			Show();
			// when we have shown it, we can obtain a window handle to disable aero peek for this window
			IntPtr windowHandle = new WindowInteropHelper(this).Handle;
			Interop.RemoveFromAeroPeek(windowHandle);
		}

		internal static void ShowWindow()
		{
			if (hidden == null)
			{
				Instance.InitWindow();
			}

			Instance.ContentContainer.Opacity = 1;
			hidden = false;
		}

		internal static void HideWindow()
		{
			if (hidden == false)
			{
				Instance.ContentContainer.Opacity = 0;
				hidden = true;
			}
		}

		public static void ShowNotification(string text, VerticalAlignment align)
		{
			if (hidden == null)
			{
				Instance.ContentContainer.Opacity = 0;
				Instance.InitWindow();
			}

			Instance.NotificationText.Text = text;
			Instance.NotificationContainer.VerticalAlignment = align;
			Storyboard sbd = (Storyboard)Instance.FindResource("NotificationFadeIn");
			sbd.Begin(Instance);
		}

		public static void HideNotification()
		{
			Storyboard sbd = (Storyboard)Instance.FindResource("NotificationFadeOut");
			sbd.Begin(Instance);
		}
	}
}
