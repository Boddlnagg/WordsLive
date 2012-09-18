using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Words.Core;
using Words.Core.Songs;
using Words.Editor;
using Words.MediaOrderList;
using Words.Presentation;
using Words.Resources;
using Words.Songs;
using Words.Utils;

namespace Words
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private MediaOrderList.MediaOrderList orderList = new MediaOrderList.MediaOrderList();
		
		private Border previewBorder = new Border();

		private FileInfo portfolioFile;

		public FileInfo PortfolioFile
		{
			get
			{
				return portfolioFile;
			}
			set
			{
				portfolioFile = value;
				OnPropertyChanged("PortfolioFile");
				OnPropertyChanged("WindowTitle");
			}
		}

		public string WindowTitle
		{
			get
			{
				return (PortfolioFile == null ? Resource.vTitleUnnamedPortfolio : PortfolioFile.Name) + " - WordsLive";
			}
		}

		IMediaControlPanel currentPanel = null;

		bool portfolioChanged = false;

		public IMediaControlPanel CurrentPanel
		{
			get
			{
				return currentPanel;
			}
			internal set
			{
				if (currentPanel != null)
					currentPanel.Close();

				currentPanel = value;
				if (currentPanel != null)
					ControlPanel.Child = currentPanel.Control;
				else
					ControlPanel.Child = null;

				this.OnPropertyChanged("CurrentPanel");
			}
		}

		public Media ActiveMedia
		{
			get
			{
				return orderList.ActiveMedia;
			}
		}

		public MainWindow()
		{
			AwesomiumManager.Init();

			this.InitializeComponent();	

			this.DataContext = this;

			orderList.ActiveItemChanged += orderList_ActiveItemChanged;
			MediaManager.MediaLoaded += MediaManager_MediaLoaded;

			this.OrderListBox.DataContext = orderList;
			this.orderList.ListChanged += (sender, args) => { portfolioChanged = true; };
			this.orderList.NotifyTryOpenFileNotFoundMedia += (sender, args) =>
			{
				MessageBox.Show("Die Datei " + args.Media.File + " existiert nicht.");
			};
			this.orderList.NotifyTryOpenUnsupportedMedia += (sender, args) =>
			{
				MessageBox.Show("Die Datei " + args.Media.File + " kann nicht angezeigt werden, da das Format nicht unterstützt wird.");
			};

			Controller.Initialize();

			if (Controller.PresentationAreaSettings.Count(s => s.IsAvailable) > 0)
			{
				Controller.UpdatePresentationAreaFromSettings();
			}
			else
			{
				ShowPresentationAreaSettingsWindow();
			}

			// Update the preview on presentation status changes ...
			Controller.PresentationManager.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Status" || args.PropertyName == "CurrentPresentation")
				{
					UpdatePreview();
				}
			};

			// ... and area changes
			Controller.PresentationManager.Area.WindowSizeChanged += (sender, args) =>
			{
				this.previewBorder.Width = Controller.PresentationManager.Area.WindowSize.Width;
				this.previewBorder.Height = Controller.PresentationManager.Area.WindowSize.Height;
			};
			this.previewBorder.Width = Controller.PresentationManager.Area.WindowSize.Width;
			this.previewBorder.Height = Controller.PresentationManager.Area.WindowSize.Height;

			if (!string.IsNullOrEmpty(App.StartupPortfolio))
			{
				OpenPortfolio(App.StartupPortfolio);
			}
		}

		void MediaManager_MediaLoaded(object sender, MediaEventArgs args)
		{
			if (args.Media is Song && UsePortfolioBackground == true)
			{
				Song song = args.Media as Song;
				for (int i = 0; i < song.Backgrounds.Count; i++)
				{
					song.Backgrounds.RemoveAt(i);
					song.Backgrounds.Insert(i, portfolioBackground);
				}
			}
		}

		void UpdatePreview()
		{
			if (Controller.PresentationManager.CurrentPresentation == null)
			{
				this.previewBorder.Background = null;
				this.PreviewBox.Child = GetPreviewControl(null);
			}
			else
			{
				this.previewBorder.Background = Brushes.Black;
				this.PreviewBox.Child = GetPreviewControl(Controller.PresentationManager.CurrentPresentation);
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (portfolioChanged)
			{
				// TODO: ask whether to save portfolio
			}
			e.Cancel = !Controller.TryCloseAllWindows();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Controller.Shutdown();
		}

		private void orderList_ActiveItemChanged(object sender, EventArgs e)
		{
			OnPropertyChanged("ActiveMedia");
			if (ActiveMedia == null)
			{
				this.CurrentPanel = null;
				Controller.PresentationManager.CurrentPresentation = null;
			}
			else
			{
				var media = ActiveMedia;
				MediaManager.LoadMedia(media);
				IMediaControlPanel panel = Controller.ControlPanels.CreatePanel(media);
				if (panel is SongControlPanel)
				{
					(panel as SongControlPanel).ShowChords = SongPresentationShowChords;
				}
				this.CurrentPanel = panel;
			}
		}

		private UIElement GetPreviewControl(IPresentation presentation)
		{
			if (presentation == null)
			{
				this.previewBorder.Child = null;
				return this.previewBorder;
			}
			else if (!presentation.Preview.IsPreviewAvailable)
			{
				this.previewBorder.Child = null;
				return new TextBlock { Text = Resource.vNoPreviewAvailable, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black };
			}
			else if (presentation.Preview is Words.Presentation.Wpf.WpfPreviewProvider)
			{
				this.previewBorder.Child = (presentation.Preview as Words.Presentation.Wpf.WpfPreviewProvider).WpfPreviewControl;
				return this.previewBorder;
			}
			else
			{
				var host = new System.Windows.Forms.Integration.WindowsFormsHost();
				host.Child = presentation.Preview.PreviewControl;
				this.previewBorder.Child = host;
				return this.previewBorder;
			}
		}

		internal void ReloadActiveMedia()
		{
			orderList.Reload(orderList.ActiveItem);
		}

		internal void TryActivateNext()
		{
			if (ActiveMedia == null)
				return;

			int index = orderList.IndexOf(orderList.ActiveItem);
			if (orderList.Count > index + 1)
			{
				orderList.ActiveItem = orderList[index + 1];
			}
		}

		private void ShowAddMediaDialog()
		{
			var typeFilters = new List<string>();
			typeFilters.Add(Resource.vFilterSupportedMediaTypes + "|" + String.Join(";", (from h in MediaManager.FileHandlers from ext in h.Extensions select "*" + ext).Distinct()));
			typeFilters.AddRange(from h in MediaManager.FileHandlers select h.Description + "|" + String.Join(";", h.Extensions.Select(s => "*" + s)));
			typeFilters.Add(Resource.vFilterAllFiles+"|*");

			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = String.Join("|", typeFilters);
			dlg.Multiselect = true;

			if (dlg.ShowDialog() == true)
			{
				if (dlg.FileNames.Count() > 1)
				{
					foreach (var m in MediaManager.LoadMultipleMediaMetadata(dlg.FileNames))
						orderList.Add(m);
				}
				else
					orderList.Add(MediaManager.LoadMediaMetadata(dlg.FileName));

				portfolioChanged = true;
			}
		}

		private void NewPortfolio()
		{
			if (portfolioChanged)
			{
				var res = MessageBox.Show(Resource.vMsgSavePortfolioChanges, Resource.vMsgSavePortfolioChangesTitle, MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;
				else if (res == MessageBoxResult.Yes)
					SavePortfolio();
			}
			orderList.Clear();
			PortfolioFile = null;
			portfolioChanged = false;
		}

		public void OpenPortfolio(string file = null)
		{
			if (portfolioChanged)
			{
				var res = MessageBox.Show(Resource.vMsgSavePortfolioChanges, Resource.vMsgSavePortfolioChangesTitle, MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;
				else if (res == MessageBoxResult.Yes)
					SavePortfolio();
			}

			if (file == null)
			{
				Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
				dlg.DefaultExt = ".ppp";
				dlg.Filter = "Powerpraise-Portfolio|*.ppp";

				if (dlg.ShowDialog() == true)
					file = dlg.FileName;
				else
					return;
			}

			IEnumerable<Media> result;
			if (MediaManager.TryLoadPortfolio(file, out result))
			{
				oldIndex = -1; // this is needed
				orderList.Clear();
				foreach (Media data in result)
					orderList.Add(data);
				PortfolioFile = new FileInfo(file);
				portfolioChanged = false;

				System.Windows.Shell.JumpList.AddToRecentCategory(file);
			}
			else
			{
				MessageBox.Show(Resource.vMsgUnableToLoadPortfolio);
			}
		}

		private void SavePortfolio()
		{
			if (PortfolioFile == null)
			{
				SavePortfolioAs();
			}
			else
			{
				MediaManager.SavePortfolio(from m in orderList select m.Data, PortfolioFile.FullName);
				portfolioChanged = false;
			}
		}

		private void SavePortfolioAs()
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.DefaultExt = ".ppp";
			dlg.Filter = "Powerpraise Portfolio|*.ppp";

			if (PortfolioFile != null)
			{
				dlg.FileName = PortfolioFile.FullName;
			}

			if (dlg.ShowDialog() == true)
			{
				MediaManager.SavePortfolio(from m in orderList select m.Data, dlg.FileName);
				PortfolioFile = new FileInfo(dlg.FileName);
				portfolioChanged = false;
			}
		}

		/// <summary>
		/// Loads a media item from a file and adds it to the current portfolio.
		/// If an item is active, the new item is added after that one, otherwise it is appended
		/// to the end of the portfolio.
		/// </summary>
		/// <param name="file">The file to add.</param>
		internal void AddToPortfolio(string file)
		{
			var media = MediaManager.LoadMediaMetadata(file);
			if (ActiveMedia != null)
			{
				int index = orderList.IndexOf(orderList.ActiveItem);
				orderList.Insert(index + 1, media);
			}
			else
			{
				orderList.Add(media);
			}
		}

		private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.Refresh)
			{
				e.CanExecute = (ActiveMedia != null);
			}
			else if (e.Command == CustomCommands.EditActive)
			{
				e.CanExecute = OrderListBox != null && (ActiveMedia as Song != null);
			}
			else if (e.Command == ApplicationCommands.Save || e.Command == ApplicationCommands.SaveAs)
			{
				e.CanExecute = (orderList.Count > 0);
			}
			else if (e.Command == CustomCommands.ShowPresentation)
			{
				e.CanExecute = Controller.PresentationManager.CurrentPresentation != null;
			}
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Open)
			{
				OpenPortfolio();
			}
			else if (e.Command == ApplicationCommands.Save)
			{
				SavePortfolio();
			}
			else if (e.Command == ApplicationCommands.SaveAs)
			{
				SavePortfolioAs();
			}
			else if (e.Command == ApplicationCommands.New)
			{
				NewPortfolio();
			}
			else if (e.Command == NavigationCommands.Refresh)
			{
				ReloadActiveMedia();
			}
			else if (e.Command == CustomCommands.Exit)
			{
				this.Close();
			}
			else if (e.Command == CustomCommands.ShowSonglist)
			{
				Controller.ShowSongList();
			}
			else if (e.Command == CustomCommands.SwitchWindow)
			{
				Controller.ShowEditorWindow();
			}
			else if (e.Command == CustomCommands.EditActive)
			{
				var song = ActiveMedia as Song;
				if (song != null)
				{
					EditorWindow win = Controller.ShowEditorWindow();
					win.LoadOrImport(song.File);
				}
			}
			else if (e.Command == CustomCommands.ShowSettings)
			{
				ShowSettingsWindow();
			}
			else if (e.Command == CustomCommands.ChoosePresentationArea)
			{
				ShowPresentationAreaSettingsWindow();
			}
			else if (e.Command == CustomCommands.AddMedia)
			{
				ShowAddMediaDialog();
			}
			else if (e.Command == CustomCommands.HidePresentation)
			{
				Controller.PresentationManager.Status = PresentationStatus.Hide;
			}
			else if (e.Command == CustomCommands.Blackscreen)
			{
				Controller.PresentationManager.Status = PresentationStatus.Blackscreen;
			}
			else if (e.Command == CustomCommands.ShowPresentation)
			{
				Controller.PresentationManager.Status = PresentationStatus.Show;
			}
			else if (e.Command == CustomCommands.ShowTestImage)
			{
				orderList.ActiveItem = null;
				Controller.PresentationManager.CurrentPresentation = Controller.PresentationManager.CreatePresentation<TestPresentation>();
				Controller.PresentationManager.Status = PresentationStatus.Show;
			}
		}

		public void ShowSettingsWindow()
		{
			var win = new SettingsWindow();
			win.Owner = this;
			win.ShowDialog();
		}

		private void ShowPresentationAreaSettingsWindow()
		{
			var win = new PresentationAreaSettingsWindow();
			win.Settings = Controller.PresentationAreaSettings;
			win.Owner = this;
			var result = win.ShowDialog();
			if (result == true)
			{
				Controller.PresentationAreaSettings = win.Settings;
				Controller.UpdatePresentationAreaFromSettings();
			}
		}

		public bool SongPresentationShowChords
		{
			get
			{
				return Properties.Settings.Default.SongPresentationShowChords;
			}
			set
			{
				if (value != Properties.Settings.Default.SongPresentationShowChords)
				{
					Properties.Settings.Default.SongPresentationShowChords = value;
					if (CurrentPanel is SongControlPanel)
					{
						(CurrentPanel as SongControlPanel).ShowChords = value;
					}
				}

				OnPropertyChanged("SongPresentationShowChords");
			}
		}

		private SongBackground portfolioBackground = null;
		private bool usePortfolioBackground;

		public bool UsePortfolioBackground
		{
			get
			{
				return usePortfolioBackground;
			}
			set
			{
				if (value != usePortfolioBackground)
				{
					usePortfolioBackground = value;

					if (usePortfolioBackground == true)
					{
						if (portfolioBackground == null)
							portfolioBackground = new SongBackground();

						var win = new ChooseBackgroundWindow(portfolioBackground);
						win.Owner = this;
						win.ShowDialog();
						if (win.DialogResult == true)
						{
							portfolioBackground = win.ChosenBackground;
							if (ActiveMedia is Song)
							{
								ReloadActiveMedia();
							}
						}
						else
						{
							usePortfolioBackground = false;
							
						}
					}
					else
					{
						// disable portfolio background
						if (ActiveMedia is Song)
						{
							ReloadActiveMedia();
						}
					}

					OnPropertyChanged("UsePortfolioBackground");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OrderListBox_OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			var selected = this.OrderListBox.SelectedItems.Cast<MediaOrderItem>();
			bool activeSelected = selected.Any() && (selected.Count((item) => item == orderList.ActiveItem) != 0);
			MediaOrderItem boundaryItem;

			if (e.Command == CustomCommands.MoveUp)
			{
				boundaryItem = orderList.Move(selected, -1);
				if (boundaryItem != null)
				{
					OrderListBox.ScrollIntoView(boundaryItem);
					portfolioChanged = true;
				}
			}
			else if (e.Command == CustomCommands.MoveDown)
			{
				boundaryItem = orderList.Move(selected, 1);
				if (boundaryItem != null)
				{
					OrderListBox.ScrollIntoView(boundaryItem);
					portfolioChanged = true;
				}
			}
			else if (e.Command == ApplicationCommands.Delete)
			{
				if (selected.Any() && (!activeSelected || MessageBox.Show("Wollen Sie das aktive Element wirklich entfernen (Die Anzeige wird auf Blackscreen geschaltet, wenn die Präsentation gerade aktiv ist)?", "Aktives Element entfernen?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes))
				{
					int index = this.OrderListBox.SelectedIndex;
					var selectedArray = selected.ToArray();
					foreach (var item in selectedArray)
					{
						orderList.Remove(item);
					}
					if (index < this.OrderListBox.Items.Count)
					{
						this.OrderListBox.SelectedIndex = index;
						OrderListBox.ScrollIntoView(this.OrderListBox.SelectedItem);
					}
					else if (this.OrderListBox.HasItems)
					{
						this.OrderListBox.SelectedIndex = this.OrderListBox.Items.Count - 1;
						OrderListBox.ScrollIntoView(this.OrderListBox.SelectedItem);
					}
					portfolioChanged = true;
				}
			}
			else if (e.Command == CustomCommands.Activate)
			{
				orderList.ActiveItem = selected.First();
			}
			else if (e.Command == NavigationCommands.Refresh)
			{
				foreach (var item in selected)
				{
					orderList.Reload(item);
				}
			}
		}

		private void OrderListBox_OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.Activate)
			{
				// can only activate a single item
				e.CanExecute = OrderListBox.SelectedItem != null && OrderListBox.SelectedItems.Count == 1;
			}
			else
			{
				e.CanExecute = OrderListBox.SelectedItem != null;
			}
		}

		int oldIndex;
		Point startPoint;

		void OrderListBox_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && oldIndex >= 0 && oldIndex < orderList.Count)
			{
				if(e.GetPosition(OrderListBox).ExceedsMinimumDragDistance(startPoint))
				{
					OrderListBox.SelectedIndex = oldIndex;
					var selectedItem = orderList[oldIndex] as MediaOrderItem;

					if (selectedItem == null)
						return;

					// this will create the drag "rectangle"
					DragDropEffects allowedEffects = DragDropEffects.Move;
					if (DragDrop.DoDragDrop(this, selectedItem, allowedEffects) != DragDropEffects.None)
					{
						// The item was dropped into a new location,
						// so make it the new selected item.
						OrderListBox.SelectedItem = selectedItem;
					}
				}
			}
		}

		void OrderListBox_Drop(object sender, DragEventArgs e)
		{
			int index = OrderListBox.GetIndexAtPosition(e.GetPosition(OrderListBox));

			// Data comes from list itself
			if (e.Data.GetData(typeof(MediaOrderItem)) != null)
			{

				if (oldIndex < 0)
					return;

				if (index == oldIndex)
					return;

				MediaOrderItem movedItem = orderList[oldIndex];

				if (index < 0)
					orderList.Move(new MediaOrderItem[] { movedItem }, orderList.Count - oldIndex - 1);
				else
					orderList.Move(new MediaOrderItem[] { movedItem }, index - oldIndex);

				oldIndex = -1;
			}
			// Data comes from explorer
			else if (e.Data.GetData(DataFormats.FileDrop) != null)
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				IEnumerable<Media> result;

				if (files.Length < 1)
					return;

				if (files.Length == 1)
				{
					if (MediaManager.TryLoadPortfolio(files[0], out result))
						Controller.OpenPortfolio(files[0]);
					else
					{
						Media m = MediaManager.LoadMediaMetadata(files[0]);
						if (index < 0)
							orderList.Add(m);
						else
							orderList.Insert(index, m);
					}
				}
				else
				{
					foreach (var m in MediaManager.LoadMultipleMediaMetadata(files))
					{
						if (index < 0)
							orderList.Add(m);
						else
							orderList.Insert(index, m);
					}
				}
			}
		}

		private void OrderListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(OrderListBox);
			oldIndex = OrderListBox.GetIndexAtPosition(p);
			if (oldIndex >= 0)
				startPoint = p;
		}
	}
}