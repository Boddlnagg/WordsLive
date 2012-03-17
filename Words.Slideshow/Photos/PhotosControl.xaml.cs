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

namespace Words.Slideshow.Photos
{
	/// <summary>
	/// Interaktionslogik für PhotosPresentationWindow.xaml
	/// </summary>
	public partial class PhotosControl : UserControl
	{
		public PhotosControl()
		{
			InitializeComponent();
		}

		public ImageSource ImageSource
		{
			get
			{
				return img.Source;
			}
			set
			{
				img.Source = value;
			}
		}
	}
}
