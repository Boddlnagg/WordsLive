/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WordsLive.Resources;

namespace WordsLive.Utils
{
	public partial class FileEntry : UserControl, IDataErrorInfo
	{
		public FileEntry()
		{
			InitializeComponent();
		}

		public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FileEntry), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(FileEntry), new PropertyMetadata(null));
		public static DependencyProperty ValidateExistsProperty = DependencyProperty.Register("ValidateExists", typeof(bool), typeof(FileEntry), new PropertyMetadata(false));


		public string Text { get { return GetValue(TextProperty) as string; } set { SetValue(TextProperty, value); } }

		public string Filter { get { return GetValue(FilterProperty) as string; } set { SetValue(FilterProperty, value); } }

		public bool ValidateExists { get { return (bool)GetValue(ValidateExistsProperty); } set { SetValue(ValidateExistsProperty, value); } }
	

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

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Text":
						if (ValidateExists && !File.Exists(Text))
							return Resource.seErrorFileDoesNotExist;
						break;
				}
				return null;
			}
		}

		public string Error
		{
			get { return null; }
		}
	}
}
