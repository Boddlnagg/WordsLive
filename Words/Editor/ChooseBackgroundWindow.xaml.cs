using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Words.Core;
using Words.Core.Songs;
using Words.Utils;
using Ionic.Zip;

namespace Words.Editor
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

			UseImage = background.IsImage;

			var root = new ObservableCollection<BackgroundsDirectory>{
				new BackgroundsDirectory {
					Info = new DirectoryInfo(MediaManager.BackgroundsDirectory),
					Parent = this,
					IsRoot = true
				}
			};
			directoryView.DataContext = root;

			if (UseImage)
			{
				if (File.Exists(Path.Combine(MediaManager.BackgroundsDirectory, background.ImagePath)))
				{
					directoryView.Loaded += (sender, args) => SelectEntry(background.ImagePath);
				}
				else
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

		internal string SelectImage;

		private void SelectEntry(string path)
		{ 
			var beginItem = (BackgroundsDirectory)directoryView.Items[0];
			SelectEntry(path, beginItem, (TreeViewItem)directoryView.ItemContainerGenerator.ContainerFromItem(beginItem));
		}

		private void SelectEntry(string remainingSelectPath, BackgroundsDirectory currentNode, TreeViewItem currentContainer)
		{
			var i = remainingSelectPath.IndexOf('\\');
			if (i >= 0)
			{
				string next = remainingSelectPath.Substring(0, i);
				remainingSelectPath = remainingSelectPath.Substring(i + 1);

				currentNode = currentNode.Directories.First((dir) => dir.Info.Name == next);
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
				SelectImage = remainingSelectPath;
				currentContainer.IsSelected = true;
				currentContainer.BringIntoView();
				imageListView.Focus();
			}
		}

		private void SetResultAndClose()
		{
			ChosenBackground = new SongBackground();
			if (UseImage)
			{
				var entry = (BackgroundEntry)imageListView.SelectedItem;
				if (entry == null)
				{
					MessageBox.Show("Es ist kein Bild ausgewählt.");
					return;
				}

				// no idea if this could happen ...
				if (!entry.File.FullName.StartsWith(MediaManager.BackgroundsDirectory))
					throw new NotSupportedException();

				// make the path relative (remove BackgroundsDirectory and slash)
				ChosenBackground.ImagePath = entry.File.FullName.Remove(0, MediaManager.BackgroundsDirectory.Length + 1);
			}
			else
			{
				ChosenBackground.Color = System.Drawing.Color.FromArgb(ColorPicker.SelectedColor.R, ColorPicker.SelectedColor.G, ColorPicker.SelectedColor.B);
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
	}

	class BackgroundEntry
	{
		public FileInfo File { get; set; }
		public Words.Utils.ImageLoader.DisplayOptions DisplayOptions { get; set; }
		public bool IsVideo { get; set; }
	}

	class BackgroundsDirectory
	{
		private static string[] allowedImageExtensions = new string[] { ".png", ".jpg", ".jpeg" };
		private static string[] allowedVideoExtensions = new string[] { ".mp4", ".wmv", ".avi" };
		private List<BackgroundEntry> images;

		public DirectoryInfo Info { get; set; }
		public ChooseBackgroundWindow Parent { get; set; }
		public bool IsRoot { get; set; }

		public List<BackgroundEntry> Images
		{
			get
			{
				if (images == null)
				{
					images = new List<BackgroundEntry>();
					LoadImages();
				}
				return images;
			}
		}

		private void LoadImages()
		{
			foreach (var file in Info.GetFiles())
			{
				BackgroundEntry entry = null;
				if (allowedImageExtensions.Contains(file.Extension.ToLower()))
				{
					entry = new BackgroundEntry { File = file, DisplayOptions = Utils.ImageLoader.DisplayOptions.Preview, IsVideo = false };
				}
				else if (allowedVideoExtensions.Contains(file.Extension.ToLower()))
				{
					entry = new BackgroundEntry { File = file, DisplayOptions = Utils.ImageLoader.DisplayOptions.VideoPreview, IsVideo = true };
				}

				if (entry != null)
				{
					images.Add(entry);
					if (Parent.SelectImage == entry.File.Name)
					{
						Parent.Dispatcher.BeginInvoke(new Action(() =>
						{
							Parent.imageListView.SelectedItem = entry;
							Parent.imageListView.ScrollIntoView(entry);
						}));
						Parent.SelectImage = null;
					}
				}
			}
		}

		private IEnumerable<BackgroundsDirectory> directories;

		public IEnumerable<BackgroundsDirectory> Directories
		{
			get
			{
				if (directories == null)
					directories = (from di in Info.GetDirectories() where di.Name != "[Thumbnails]"
								   select new BackgroundsDirectory { Info = di, Parent = this.Parent }).ToList();
				return directories;
			}
		}
	}
}
