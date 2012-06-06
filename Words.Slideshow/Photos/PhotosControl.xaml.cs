using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Words.Slideshow.Photos
{
	/// <summary>
	/// Interaktionslogik für PhotosPresentationWindow.xaml
	/// </summary>
	public partial class PhotosControl : UserControl
	{
		Storyboard storyboard;
		int transitionDuration = 500; // in milliseconds

		public PhotosControl()
		{
			InitializeComponent();
			DoubleAnimation ani = new DoubleAnimation { From = 0.0, To = 1.0 };
			storyboard = new Storyboard();
			storyboard.Children.Add(ani);
			Storyboard.SetTarget(ani, front);
			Storyboard.SetTargetProperty(ani, new PropertyPath(Image.OpacityProperty));
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

		public ImageSource ImageSource
		{
			get
			{
				return front.Source;
			}
			set
			{
				if (value != front.Source)
				{
					back.Source = front.Source;
					front.Source = value;
					storyboard.Children[0].Duration = new TimeSpan(0, 0, 0, 0, transitionDuration);
					storyboard.Begin(this);
				}
			}
		}
	}
}
