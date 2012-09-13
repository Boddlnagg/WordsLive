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
		bool animationRunning;
		ImageInfo current;
		ImageInfo next;

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
			if (next != null)
			{
				Update(next);
				next = null;
			}
			else
			{
				OnLoadingFinished();
			}
		}

		public event EventHandler LoadingFinished;

		protected virtual void OnLoadingFinished()
		{
			if (LoadingFinished != null)
				LoadingFinished(this, EventArgs.Empty);
		}

		public ImageInfo CurrentImage
		{
			get
			{
				return current;
			}
			set
			{
				if (value != current)
				{
					current = value;

					if (animationRunning)
					{
						next = value;
					}
					else
					{
						Update(value);
					}
				}
			}
		}

		private void back_Loaded(object sender, RoutedEventArgs e)
		{
			animationRunning = true;
			storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.ImageTransition);
			storyboard.Begin(this);
		}

		private void Update(ImageInfo image)
		{
			Loader.SetSourceType(back, image.SourceType);
			Loader.SetSource(back, image.Source);
		}
	}
}
