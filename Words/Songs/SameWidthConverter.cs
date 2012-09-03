using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Words.Utils;

namespace Words.Songs
{
	[ValueConversion(typeof(ItemsControl), typeof(double))]
	public class SameWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			ItemsControl obj = (ItemsControl)value;
			ContentPresenter cp = obj.ItemContainerGenerator.ContainerFromIndex(0).FindVisualChild<ContentPresenter>();
			FrameworkElement element = (FrameworkElement)cp.ContentTemplate.FindName((string)parameter, cp);
			if (element != null)
			{
				double max = element.ActualWidth;
				for (int i = 1; i < obj.Items.Count; i++)
				{
					cp = obj.ItemContainerGenerator.ContainerFromIndex(i).FindVisualChild<ContentPresenter>();
					element = (FrameworkElement)cp.ContentTemplate.FindName((string)parameter, cp);
					if (element.ActualWidth > max)
						max = element.ActualWidth;
				}
				return max;
			}
			else
			{
				GroupItem groupItem;
				int i = 0;
				double max = 0;
				while ((groupItem = (GroupItem)obj.FindVisualChild<GroupItem>(i++)) != null)
				{
					element = (FrameworkElement)groupItem.Template.FindName((string)parameter, groupItem);
					if (element.ActualWidth > max)
						max = element.ActualWidth;
				}
				return max;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
