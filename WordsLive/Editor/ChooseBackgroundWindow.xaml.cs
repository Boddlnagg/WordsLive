using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WordsLive.Core.Data;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.Storage;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class ChooseBackgroundWindow : Window, INotifyPropertyChanged
	{
		public SongBackground ChosenBackground
		{
			get;
			private set;
		}

		private bool useImage;

		public bool UseImage
		{
			get
			{
				return useImage;
			}
			set
			{
				useImage = value;
				OnNotifyPropertyChanged("UseImage");
				OnNotifyPropertyChanged("UseColor");
			}
		}

		public bool UseColor
		{
			get
			{
				return !useImage;
			}
			set
			{
				useImage = !value;
				OnNotifyPropertyChanged("UseImage");
				OnNotifyPropertyChanged("UseColor");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string property)
		{ 
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}


		public ChooseBackgroundWindow(SongBackground background)
		{
			InitializeComponent();

			this.DataContext = this;

			UseImage = background.IsFile;

			directoryView.DataContext = new BackgroundDirectory[] { DataManager.Backgrounds.Root };

			if (background.IsFile)
			{
				try
				{
					// try GetFile in order to find out if file exists
					var file = DataManager.Backgrounds.GetFile(background);
					directoryView.Loaded += (sender, args) => SelectEntry(background.FilePath);
				}
				catch (FileNotFoundException)
				{
					MessageBox.Show("Die Hintergrundbilddatei wurde nicht gefunden und wird durch einen schwarzen Hintergrund ersetzt.");
					UseColor = true;
					ColorPicker.SelectedColor = Colors.Black;
				}
			}
			else
			{
				ColorPicker.SelectedColor = Color.FromRgb(background.Color.R, background.Color.G, background.Color.B);
			}
		}

		private void SelectEntry(string path)
		{ 
			var beginItem = (BackgroundDirectory)directoryView.Items[0];
			SelectEntry(path, beginItem, (TreeViewItem)directoryView.ItemContainerGenerator.ContainerFromItem(beginItem));
		}

		private void SelectEntry(string remainingSelectPath, BackgroundDirectory currentNode, TreeViewItem currentContainer)
		{
			var i = remainingSelectPath.IndexOf('\\');
			if (i >= 0)
			{
				string next = remainingSelectPath.Substring(0, i);
				remainingSelectPath = remainingSelectPath.Substring(i + 1);

				currentNode = currentNode.Directories.First((dir) => dir.Name == next);
				currentContainer = currentContainer.ItemContainerGenerator.ContainerFromItem(currentNode) as TreeViewItem;
				currentContainer.IsExpanded = true;
				currentContainer.ItemContainerGenerator.StatusChanged += (sender, args) =>
				{
					if (currentContainer.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
						SelectEntry(remainingSelectPath, currentNode, currentContainer);
				};
			}
			else
			{
				imageListView.SelectedItem = currentNode.Files.Where(file => file.Name == remainingSelectPath).SingleOrDefault();
				currentContainer.IsSelected = true;
				currentContainer.BringIntoView();
				imageListView.Focus();
			}
		}

		private void SetResultAndClose()
		{
			if (UseImage)
			{
				var entry = (BackgroundFile)imageListView.SelectedItem;
				if (entry == null)
				{
					MessageBox.Show("Es ist kein Bild ausgewählt.");
					return;
				}

				ChosenBackground = new SongBackground(entry.Path.Substring(1).Replace('/', '\\'), entry.IsVideo);
			}
			else
			{
				ChosenBackground = new SongBackground(System.Drawing.Color.FromArgb(ColorPicker.SelectedColor.R, ColorPicker.SelectedColor.G, ColorPicker.SelectedColor.B));
			}

			this.DialogResult = true;
			this.Close();
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			SetResultAndClose();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				SetResultAndClose();
			}
		}

		private void directoryView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			ScrollViewer scrollViewer = imageListView.FindVisualChild<ScrollViewer>();
			scrollViewer.ScrollToTop();
		}

		private void image_ImageLoaderLoaded(object sender, RoutedEventArgs e)
		{
			var cp = (sender as DependencyObject).FindVisualParent<ContentPresenter>();
			if (cp.Content.Equals(imageListView.SelectedItem))
			{
				imageListView.ScrollIntoView(imageListView.SelectedItem);
			}
		}
	}
}
