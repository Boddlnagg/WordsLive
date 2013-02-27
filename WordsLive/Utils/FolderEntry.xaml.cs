using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WordsLive.Resources;
using WinForms = System.Windows.Forms;

namespace WordsLive.Utils
{
	public partial class FolderEntry : UserControl, IDataErrorInfo
	{
		public FolderEntry()
		{
			InitializeComponent();
		}

		public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FolderEntry), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FolderEntry), new PropertyMetadata(null));
		public static DependencyProperty ValidateExistsProperty = DependencyProperty.Register("ValidateExists", typeof(bool), typeof(FolderEntry), new PropertyMetadata(false));


		public string Text { get { return GetValue(TextProperty) as string; } set { SetValue(TextProperty, value); } }

		public string Description { get { return GetValue(DescriptionProperty) as string; } set { SetValue(DescriptionProperty, value); } }

		public bool ValidateExists { get { return (bool)GetValue(ValidateExistsProperty); } set { SetValue(ValidateExistsProperty, value); } }


		private void BrowseFolder(object sender, RoutedEventArgs e)
		{
			using (var dlg = new WinForms.FolderBrowserDialog())
			{
				dlg.Description = Description;
				dlg.SelectedPath = Text;
				dlg.ShowNewFolderButton = true;
				var result = dlg.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					Text = dlg.SelectedPath;
					BindingExpression be = GetBindingExpression(TextProperty);
					if (be != null)
						be.UpdateTarget();
				}
			}
		}

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Text":
						if (ValidateExists && !Directory.Exists(Text))
							return Resource.seErrorFolderDoesNotExist;
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
