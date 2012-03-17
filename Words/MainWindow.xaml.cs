using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Words.Core;
using Words.Core.Songs;
using Words.Editor;
using Words.MediaOrderList;
using Words.Presentation;
using System.Linq;
using Words.Songs;

namespace Words
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private Words.MediaOrderList.MediaOrderList orderList = new Words.MediaOrderList.MediaOrderList();
		
		private Border previewBorder = new Border();

		string portfolioFilename;

		IMediaControlPanel currentPanel = null;

		public IMediaControlPanel CurrentPanel
		{
			get
			{
				return currentPanel;
			}
			private set
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

		public MainWindow()
		{
			AwesomiumManager.Init();

			this.InitializeComponent();

			this.DataContext = this;

			DependencyPropertyDescriptor activeItemDescriptor = DependencyPropertyDescriptor.FromProperty(MediaOrderListBox.ActiveItemProperty, typeof(MediaOrderListBox));

			if (activeItemDescriptor != null)
			{
				activeItemDescriptor.AddValueChanged(this.OrderListBox, OrderListBox_ActiveItemChanged);
			}

			this.OrderListBox.Init(orderList);

			Controller.Initialize();

			if (Controller.PresentationAreaSettings.Count(s => s.IsAvailable) > 0)
			{
				UpdatePresentationAreaFromSettings();
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

		void UpdatePreview()
		{
			switch (Controller.PresentationManager.Status)
			{
				case PresentationStatus.Hide:
					this.previewBorder.Background = null;
					this.PreviewBox.Child = GetPreviewControl(null);
					break;
				case PresentationStatus.Blackscreen:
					this.previewBorder.Background = Brushes.Black;
					this.PreviewBox.Child = GetPreviewControl(Controller.PresentationManager.Blackscreen);
					break;
				case PresentationStatus.Show:
					this.previewBorder.Background = Brushes.Black;
					this.PreviewBox.Child = GetPreviewControl(Controller.PresentationManager.CurrentPresentation);
					break;
			}
		}

		private void OrderListButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string tag = (sender as Control).Tag as string;
			var selected = this.OrderListBox.SelectedItems.Cast<ActivatableItemContainer<MediaOrderItem>>();
			ActivatableItemContainer<MediaOrderItem> boundaryItem;

			switch (tag)
			{ 
				case "MoveUp":
					boundaryItem = orderList.Move(selected, -1);
					if (boundaryItem != null)
						OrderListBox.ScrollIntoView(boundaryItem);
					break;
				case "MoveDown":
					boundaryItem = orderList.Move(selected, 1);
					if (boundaryItem != null)
						OrderListBox.ScrollIntoView(boundaryItem);
					break;
				case "Delete":
					if (selected.Count() > 0 && (selected.Count((item) => item.IsActive) == 0 || MessageBox.Show("Wollen Sie das aktive Element wirklich entfernen (Die Anzeige wird auf Blackscreen geschaltet, wenn die Präsentation gerade aktiv ist)?", "Aktives Element entfernen?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes))
					{
						int index = this.OrderListBox.SelectedIndex;
						var selectedArray = selected.ToArray();
						foreach(var item in selectedArray)
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
					}
					break;
			}
			
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = !Controller.TryCloseAllWindows();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Controller.Shutdown();
		}

		private void OrderListBox_ActiveItemChanged(object sender, EventArgs e)
		{
			if (orderList.ActiveItem == null)
			{
				if (Controller.PresentationManager.Status == PresentationStatus.Show)
				{
					Controller.PresentationManager.Status = PresentationStatus.Blackscreen;
				}

				if (Controller.PresentationManager.CurrentPresentation != null)
				{
					Controller.PresentationManager.CurrentPresentation.Close();
					Controller.PresentationManager.CurrentPresentation = null;
				}

				this.CurrentPanel = null;
			}
			else
			{
				var media = OrderListBox.ActiveItem.Data;
				LoadMedia(media);
				IMediaControlPanel panel = Controller.ControlPanels.CreatePanel(media);
				if (panel is SongControlPanel)
				{
					(panel as SongControlPanel).ShowChords = SongPresentationShowChords;
				}
				this.CurrentPanel = panel;
			}
		}

		private Media LoadMedia(Media media)
		{
			media.Load();

			if (!media.IsLoaded)
				return null;

			if (media is Song && UsePortfolioBackground == true)
			{
				Song song = media as Song;
				for (int i = 0; i < song.Backgrounds.Count; i++)
				{
					song.Backgrounds.RemoveAt(i);
					song.Backgrounds.Insert(i, portfolioBackground);
				}
			}

			return media;
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
				return new TextBlock { Text = "Keine Vorschau verfügbar", TextAlignment = TextAlignment.Center, Foreground = Brushes.Black };
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
			if (OrderListBox.ActiveItem == null)
				return;

			if (CurrentPanel != null && CurrentPanel.IsUpdatable && File.Exists(OrderListBox.ActiveItem.Path))
			{
				OrderListBox.ActiveItem.Reload();
				CurrentPanel.Init(LoadMedia(OrderListBox.ActiveItem.Data));
			}
			else
			{
				var active = OrderListBox.ActiveItem.Data;

				if (String.IsNullOrEmpty(active.File))
					return; // can't reload when no filename is given

				var newData = MediaManager.LoadMediaMetadata(active.File);

				CurrentPanel = null;

				// needed to restore selected index
				int selected = OrderListBox.SelectedIndex;

				orderList.ReplaceActiveBy(newData);
				if (OrderListBox.SelectedIndex != selected)
					OrderListBox.SelectedIndex = selected;
			}
		}

		private void ShowAddMediaDialog()
		{
			var typeFilters = new List<string>();
			typeFilters.Add("Unterstützte Medientypen|" + String.Join(";", from t in MediaManager.MediaTypes from ext in t.Extensions select "*" + ext));
			typeFilters.AddRange(from t in MediaManager.MediaTypes select t.Description + "|" + String.Join(";", t.Extensions.Select(s => "*" + s)));
			typeFilters.Add("Alle Dateien|*");

			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = String.Join("|", typeFilters);
			dlg.Multiselect = true;

			if (dlg.ShowDialog() == true)
			{
				foreach (var file in dlg.FileNames)
				{
					orderList.Add(MediaManager.LoadMediaMetadata(file));
				}
			}
		}

		public void OpenPortfolio(string file = null)
		{
			if (orderList.Count > 0)
			{
				// TODO (Words): Sicherheitsabfrage, falls verändert
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
				orderList.Clear();
				foreach (Media data in result)
					orderList.Add(data);
				portfolioFilename = file;

				System.Windows.Shell.JumpList.AddToRecentCategory(file);
			}
			else
			{
				MessageBox.Show("Portfolio konnte nicht geladen werden.");
			}
		}

		private void SavePortfolio()
		{
			if (string.IsNullOrEmpty(portfolioFilename))
			{
				SavePortfolioAs();
			}
			else
			{
				MediaManager.SavePortfolio(from m in orderList select m.Data.Data, portfolioFilename);
			}
		}

		private void SavePortfolioAs()
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.DefaultExt = ".ppp";
			dlg.Filter = "Powerpraise Portfolio|*.ppp";

			if (portfolioFilename != null)
			{
				dlg.FileName = portfolioFilename;
			}

			if (dlg.ShowDialog() == true)
			{
				MediaManager.SavePortfolio(from m in orderList select m.Data.Data, dlg.FileName);
				portfolioFilename = dlg.FileName;
			}
		}

		private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.Refresh)
			{
				e.CanExecute = (OrderListBox.ActiveItem != null);
			}
			else if (e.Command == CustomCommands.EditActive)
			{
				e.CanExecute = (orderList.ActiveItem != null) && (orderList.ActiveItem.Data.Data as Song != null);
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
				// TODO (Words): Sicherheitsabfrage, falls verändert
				orderList.Clear();
				portfolioFilename = null;
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
				if (orderList.ActiveItem != null)
				{
					var song = orderList.ActiveItem.Data.Data as Song;
					if (song != null)
					{
						EditorWindow win = Controller.ShowEditorWindow();
						win.Load(song.File);
					}
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
				UpdatePresentationAreaFromSettings();
			}
		}

		private void UpdatePresentationAreaFromSettings()
		{
			var setting = Controller.PresentationAreaSettings.First(s => s.IsAvailable);
			Controller.PresentationManager.Area.ScreenIndex = (int)setting.ScreenIndex;
			Controller.PresentationManager.Area.Fullscreen = setting.Fullscreen;
			Controller.PresentationManager.Area.Offset = new System.Drawing.Point(setting.Left, setting.Top);
			Controller.PresentationManager.Area.Size = new System.Drawing.Size(setting.Width, setting.Height);
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
							if (orderList.ActiveItem != null && orderList.ActiveItem.Data.Data is Song)
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
						if (orderList.ActiveItem != null && orderList.ActiveItem.Data.Data is Song)
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
	}
}