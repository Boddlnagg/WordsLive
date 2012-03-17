using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Words.Songs
{
	[ValueConversion(typeof(SongSlideListBox), typeof(double))]
	public class SameWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			SongSlideListBox obj = (SongSlideListBox)value;
			ListBoxItem listBoxItem = (ListBoxItem)obj.ItemContainerGenerator.ContainerFromIndex(0);
			ContentPresenter cp = FindVisualChild<ContentPresenter>(listBoxItem, 0);
			FrameworkElement element = (FrameworkElement)cp.ContentTemplate.FindName((string)parameter, cp);
			if (element != null)
			{
				double max = element.ActualWidth;
				for (int i = 1; i < obj.Items.Count; i++)
				{
					listBoxItem = (ListBoxItem)obj.ItemContainerGenerator.ContainerFromIndex(i);
					cp = FindVisualChild<ContentPresenter>(listBoxItem, 0);
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
				while ((groupItem = (GroupItem)FindVisualChild<GroupItem>(obj, i++)) != null)
				{
					element = (FrameworkElement)groupItem.Template.FindName((string)parameter, groupItem);
					if (element.ActualWidth > max)
						max = element.ActualWidth;
				}
				return max;
			}
		}

		private childItem FindVisualChild<childItem>(DependencyObject obj, int index) where childItem : DependencyObject
		{
			int count = 0;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is childItem)
				{
					if (count == index)
						return (childItem)child;
					else
						count++;
				}
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child, index);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}


		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
