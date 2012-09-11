using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Words.Utils.ImageLoader;

namespace Words.Images
{
	public partial class ImagesControl : UserControl
	{
		Storyboard storyboard;
		int transitionDuration = 500; // in milliseconds
		bool animationRunning;
		string nextSource = null;

		public ImagesControl()
		{
			InitializeComponent();
			DoubleAnimation ani = new DoubleAnimation { From = 1.0, To = 0.0, FillBehavior = FillBehavior.Stop };
			ani.Completed += ani_Completed;
			Storyboard.SetTarget(ani, frontGrid);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));

			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
		}

		void ani_Completed(object sender, EventArgs e)
		{
			front.Source = back.Source;
			frontGrid.Opacity = 1;
			animationRunning = false;
			if (nextSource != null)
			{
				Loader.SetSource(back, nextSource);
				nextSource = null;
			}
		}

		public int TransitionDuration
		{
			get 
			{
				return transitionDuration;
			}
			set
			{
				transitionDuration = value;
			}
		}

		public string ImageSource
		{
			get
			{
				return (string)Loader.GetSource(front);
			}
			set
			{
				if (value != (string)Loader.GetSource(back))
				{
					if (animationRunning)
					{
						nextSource = value;
					}
					else
					{
						Loader.SetSource(back, value);
					}
				}
			}
		}

		private void back_Loaded(object sender, RoutedEventArgs e)
		{
			animationRunning = true;
			storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, transitionDuration);
			storyboard.Begin(this);
		}
	}
}
