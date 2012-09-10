using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Windows.Controls;
using Words.Core;

namespace Words.Presentation.Wpf
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

		[Shutdown]
		public static void Shutdown()
		{
			if (Instance != null)
				Instance.Close();
		}

		/// <summary>
		/// Sets another control as the new content and starts a transition.
		/// </summary>
		/// <param name="control">The control to show next.</param>
		/// <param name="milliseconds">The duration of the transition in milliseconds.</param>
		public static void SetContent(FrameworkElement control, int milliseconds, Action callback, IPresentation previous)
		{
			if (Instance.MainContainer.Child == control && previous is IWpfPresentation)
				return;

			if (contentTransitionCallback != null)
				contentTransitionCallback();

			contentTransitionCallback = callback;

			Instance.PreviousContainer.Child = null;
			UIElement previousControl;
			if (previous is IWpfPresentation)
				previousControl = Instance.MainContainer.Child;
			else
				previousControl = null;
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

			Storyboard sbd = (Storyboard)Instance.FindResource("ContentFadeOut");
			sbd.Children[0].Duration = new TimeSpan(0, 0, 0, 0, milliseconds);
			sbd.Begin(Instance);
		}
		
		private static Action contentTransitionCallback = null;

		private void ContentTransition_Completed(object sender, EventArgs e)
		{
			Instance.PreviousContainer.Child = null;
			Instance.PreviousContainer.Opacity = 1;

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

		internal static void ShowWindow()
		{
			if (hidden == null)
			{
				// the window has not been shown before, so show it
				Instance.Show();
				// when we have shown it, we can obtain a window handle to disable aero peek for this window
				IntPtr windowHandle = new WindowInteropHelper(Instance).Handle;
				AeroPeekHelper.RemoveFromAeroPeek(windowHandle);
			}

			Instance.ContentContainer.Opacity = 1;

			//Instance.Left = Instance.area.WindowLocation.X;
			//Instance.Top = Instance.area.WindowLocation.Y;

			hidden = false;
		}

		internal static void HideWindow()
		{
			if (hidden == false)
			{
				Instance.ContentContainer.Opacity = 0;
				//Instance.Left = -32000;
				//Instance.Top = -32000;
				hidden = true;
			}
		}
	}
}
