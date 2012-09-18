using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Presentation.Wpf;
using System.Windows.Controls;
using System.Windows.Media;

namespace WordsLive
{
	public class TestPresentation : WpfPresentation<Border>
	{
		public TestPresentation()
		{
			Control.BorderThickness = new System.Windows.Thickness(4);
			Control.BorderBrush = Brushes.Red;
			Control.Background = Brushes.White;
		}
	}
}
