using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Words.Utils
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Validate all dependency objects in a window
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static bool IsValid(this DependencyObject node)
		{
			// Check if dependency object was passed
			if (node != null)
			{
				// Check if dependency object is valid.
				// NOTE: Validation.GetHasError works for controls that have validation rules attached 
				bool isValid = !Validation.GetHasError(node);
				if (!isValid)
				{
					// If the dependency object is invalid, and it can receive the focus,
					// set the focus
					if (node is IInputElement) Keyboard.Focus((IInputElement)node);
					return false;
				}
			}

			// If this dependency object is valid, check all child dependency objects
			foreach (object subnode in LogicalTreeHelper.GetChildren(node))
			{
				if (subnode is DependencyObject)
				{
					// If a child dependency object is invalid, return false immediately,
					// otherwise keep checking
					if (IsValid((DependencyObject)subnode) == false) return false;
				}
			}

			// All dependency objects are valid
			return true;
		}

		static public bool SetSelectedItem(this TreeView treeView, object item)
		{
			return SetSelected(treeView, item);
		}

		static private bool SetSelected(ItemsControl parent, object child)
		{
			if (parent == null || child == null)
			{
				return false;
			}

			TreeViewItem childNode = parent.ItemContainerGenerator
				.ContainerFromItem(child) as TreeViewItem;

			if (childNode != null)
			{
				childNode.Focus();
				return childNode.IsSelected = true;
			}

			if (parent.Items.Count > 0)
			{
				foreach (object childItem in parent.Items)
				{
					ItemsControl childControl = parent
						.ItemContainerGenerator
						.ContainerFromItem(childItem)
						as ItemsControl;

					(childControl as TreeViewItem).IsExpanded = true;

					if (SetSelected(childControl, child))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static DependencyObject VisualUpwardSearch<T>(this DependencyObject source)
		{
			while (source != null && source.GetType() != typeof(T))
				source = VisualTreeHelper.GetParent(source);

			return source;
		}

		public static T FindVisualChild<T>(this DependencyObject source) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(source, i);

				if (child != null && child is T)
					return (T)child;
				else
				{
					T childOfChild = FindVisualChild<T>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}

			return null;
		}

		public static int GetCurrentIndex(this ListBox listBox, GetPositionDelegate getPosition)
		{
			int index = -1;
			for (int i = listBox.Items.Count - 1; i >= 0; i--)
			{
				ListBoxItem item = GetListBoxItem(listBox, i);
				if (item != null && IsMouseOverTarget(item, getPosition))
				{
					index = i;
					break;
				}
			}
			return index;
		}

		private  static ListBoxItem GetListBoxItem(this ListBox listBox, int index)
		{
			if (listBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
				return null;

			return listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
		}

		private static bool IsMouseOverTarget(this Visual target, GetPositionDelegate getPosition)
		{
			Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
			Point mousePos = getPosition((IInputElement)target);
			return bounds.Contains(mousePos);
		}

		public delegate Point GetPositionDelegate(IInputElement element);

		public static bool ContainsIgnoreCase(this string str, string value)
		{
			return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static T GetItemAtLocation<T>(this UIElement visual, Point location) where T : DependencyObject
		{
			DependencyObject obj = visual.InputHitTest(location) as DependencyObject;
			return (T)obj.VisualUpwardSearch<T>();
		}
		
		public static void CreateRecursive(this DirectoryInfo dirInfo)
		{
			if (dirInfo.Parent != null && !dirInfo.Exists)
				CreateRecursive(dirInfo.Parent);

			if (!dirInfo.Exists)
			{
				dirInfo.Create();
				dirInfo.Refresh();
			}
		}
	}
}
