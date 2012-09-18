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

namespace WordsLive.Utils
{
	public partial class FileEntry : UserControl
	{
		public FileEntry()
		{
			InitializeComponent();
		}

		public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FileEntry), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(FileEntry), new PropertyMetadata(null));

		public string Text { get { return GetValue(TextProperty) as string; } set { SetValue(TextProperty, value); } }

		public string Filter { get { return GetValue(FilterProperty) as string; } set { SetValue(FilterProperty, value); } }


		private void BrowseFile(object sender, RoutedEventArgs e)
		{
			var dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Multiselect = false;
			dlg.FileName = Text;
			dlg.Filter = Filter;
			if (dlg.ShowDialog() == true)
			{
				Text = dlg.FileName;
				BindingExpression be = GetBindingExpression(TextProperty);
				if (be != null)
					be.UpdateTarget();
			}
		}

	}
}
