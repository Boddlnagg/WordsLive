using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Words.Utils;

namespace Words.MediaOrderList
{
	public static class ActivatableListBox
	{
		[AttachedPropertyBrowsableForType(typeof(ListBox))]
		public static bool GetIsActivatable(ListBox obj)
		{
			return (bool)obj.GetValue(IsActivatableProperty);
		}

		public static void SetIsActivatable(ListBox obj, bool value)
		{
			obj.SetValue(IsActivatableProperty, value);
		}

		public static readonly DependencyProperty IsActivatableProperty =
			DependencyProperty.RegisterAttached("IsActivatable", typeof(bool), typeof(ActivatableListBox), new UIPropertyMetadata(false, OnIsActivatableChanged));

		private static void OnIsActivatableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var value = (bool)e.NewValue;
			var sender = d as ListBox;

			if (value)
			{
				sender.MouseDoubleClick += ListBox_MouseDoubleClick;
				sender.ItemContainerGenerator.StatusChanged += ListBox_ContainerGeneratorStatusChanged;
			}
			else
			{
				sender.MouseDoubleClick -= ListBox_MouseDoubleClick;
				sender.ItemContainerGenerator.StatusChanged -= ListBox_ContainerGeneratorStatusChanged;
			}
		}

		static void ListBox_ContainerGeneratorStatusChanged(object sender, EventArgs e)
		{
			var generator = sender as ItemContainerGenerator;
			if (generator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
			{
				var first = generator.ContainerFromIndex(0) as DependencyObject;
				if (first != null)
				{
					// find parent ListBox from first container
					var listBox = (ListBox)first.VisualUpwardSearch<ListBox>();
					var value = listBox.GetActiveItem();
					UpdateActivatedItem(listBox, value);
				}
			}
		}

		private static void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				// get ListBoxItem at mouse position
				var listBox = sender as ListBox;
				var element = listBox.InputHitTest(e.GetPosition(listBox)) as DependencyObject;
				var listBoxItem = element.VisualUpwardSearch<ListBoxItem>();
				if (listBoxItem != null)
				{
					var content = listBoxItem.GetValue(ListBoxItem.ContentProperty) as IActivatable;
					if (content != null)
					{
						if (content.Activate())
						{
							SetActiveItem(listBox, content);
						}
					}
				}
			}
		}

		[AttachedPropertyBrowsableForType(typeof(ListBox))]
		public static IActivatable GetActiveItem(this ListBox obj)
		{
			return (IActivatable)obj.GetValue(ActiveItemProperty);
		}

		public static void SetActiveItem(this ListBox obj, IActivatable value)
		{
			obj.SetValue(ActiveItemProperty, value);
		}

		public static readonly DependencyProperty ActiveItemProperty =
			DependencyProperty.RegisterAttached("ActiveItem", typeof(IActivatable), typeof(ActivatableListBox), new UIPropertyMetadata(null, OnActiveItemChanged));

		private static void OnActiveItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var value = e.NewValue as IActivatable;
			var listBox = d as ListBox;
			UpdateActivatedItem(listBox, value);
		}

		[AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
		public static bool GetIsActive(ListBoxItem obj)
		{
			return (bool)obj.GetValue(IsActiveProperty);
		}

		public static readonly DependencyPropertyKey IsActivePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsActive", typeof(bool), typeof(ActivatableListBox), new UIPropertyMetadata(false));

		public static readonly DependencyProperty IsActiveProperty =
			IsActivePropertyKey.DependencyProperty;

		private static void UpdateActivatedItem(ListBox listBox, IActivatable value)
		{
			foreach (var item in listBox.Items)
			{
				var container = listBox.ItemContainerGenerator.ContainerFromItem(item);
				if (container != null)
					container.SetValue(IsActivePropertyKey, item == value);
			}
		}
	}
}
