/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WordsLive.Awesomium;
using WordsLive.Core;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.Storage;
using WordsLive.Editor;
using WordsLive.MediaOrderList;
using WordsLive.Presentation;
using WordsLive.Resources;
using WordsLive.Songs;
using WordsLive.Utils;

namespace WordsLive
{
	// TODO: simplify this class by splitting it up
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

		bool portfolioModified = false;

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
				{
					ControlPanel.Child = currentPanel.Control;
					ControlPanel.Child.Focus();
				}
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
			this.orderList.ListChanged += (sender, args) => { portfolioModified = true; };

			Controller.Initialize();
		}

		void MediaManager_MediaLoaded(object sender, MediaEventArgs args)
		{
			if (args.Media is SongMedia)
			{
				Song song = (args.Media as SongMedia).Song;

				if (UsePortfolioBackground == true)
				{	
					for (int i = 0; i < song.Backgrounds.Count; i++)
					{
						song.Backgrounds.RemoveAt(i);
						song.Backgrounds.Insert(i, portfolioBackground);
					}
				}

				if (Properties.Settings.Default.TemplateMasterEnable)
				{
					ApplySongTemplateMaster(song, MasterOverrideOptions.CreateFromSettings());
				}
			}
		}

		void ApplySongTemplateMaster(Song song, MasterOverrideOptions options)
		{
			SongFormatting master = Song.CreateFromTemplate().Formatting;
			var changed = song.Formatting;

			if (options.TextFormatting)
			{
				changed.MainText = master.MainText;
				changed.TranslationText = master.TranslationText;
			}
			if (options.TextPosition)
			{
				changed.HorizontalOrientation = master.HorizontalOrientation;
				changed.VerticalOrientation = master.VerticalOrientation;
				changed.TextLineSpacing = master.TextLineSpacing;
				changed.TranslationPosition = master.TranslationPosition;
				changed.TranslationLineSpacing = master.TranslationLineSpacing;
			}
			if (options.SourceFormatting)
			{
				changed.SourceText = master.SourceText;
			}
			if (options.SourcePosition)
			{
				changed.SourceDisplayPosition = master.SourceDisplayPosition;
				changed.SourceBorderRight = master.SourceBorderRight;
				changed.SourceBorderTop = master.SourceBorderTop;
			}
			if (options.CopyrightFormatting)
			{
				changed.CopyrightText = master.CopyrightText;
			}
			if (options.CopyrightPosition)
			{
				changed.CopyrightDisplayPosition = master.CopyrightDisplayPosition;
				changed.CopyrightBorderBottom = master.CopyrightBorderBottom;
			}
			if (options.OutlineShadow)
			{
				changed.IsOutlineEnabled = master.IsOutlineEnabled;
				changed.IsShadowEnabled = master.IsShadowEnabled;
				changed.OutlineColor = master.OutlineColor;
				changed.ShadowColor = master.ShadowColor;
				changed.ShadowDirection = master.ShadowDirection;
			}

			song.Formatting = changed;
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
			if (portfolioModified)
			{
				NewPortfolio();
			}

			// it might still be modified if saving was cancelled
			if (portfolioModified)
			{
				e.Cancel = true;
			}
			else
			{
				e.Cancel = !Controller.TryCloseAllWindows();
			}
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
				try
				{
					MediaManager.LoadMedia(media);
					IMediaControlPanel panel = Controller.ControlPanels.CreatePanel(media);
					if (panel is SongControlPanel)
					{
						(panel as SongControlPanel).ShowChords = SongPresentationShowChords;
					}
					this.CurrentPanel = panel;
				}
				catch (FileNotFoundException)
				{
					var newData = new FileNotFoundMedia(media.Uri);
					orderList.ReplaceActive(newData);
				}
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
			else if (presentation.Preview is WordsLive.Presentation.Wpf.WpfPreviewProvider)
			{
				this.previewBorder.Child = (presentation.Preview as WordsLive.Presentation.Wpf.WpfPreviewProvider).WpfPreviewControl;
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

		internal void TryActivateOffset(int offset)
		{
			if (ActiveMedia == null)
				return;

			int newIndex = orderList.IndexOf(orderList.ActiveItem) + offset;
			if (newIndex >= 0 && newIndex < orderList.Count)
			{
				orderList.ActiveItem = orderList[newIndex];
			}
		}

		private void ShowAddMediaDialog()
		{
			var typeFilters = new List<string>();
			typeFilters.Add(Resource.vFilterSupportedMediaTypes + "|" + String.Join(";", (from h in MediaManager.Handlers from ext in h.Extensions select "*" + ext).Distinct()));
			typeFilters.AddRange(from h in MediaManager.Handlers select h.Description + "|" + String.Join(";", h.Extensions.Select(s => "*" + s)));
			typeFilters.Add(Resource.vFilterAllFiles+"|*");

			var dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Title = Resource.vMenuAddMedia;
			dlg.Filter = String.Join("|", typeFilters);
			dlg.Multiselect = true;
			dlg.InitialDirectory = Properties.Settings.Default.LastMediaDirectory;

			if (dlg.ShowDialog() == true)
			{
				Properties.Settings.Default.LastMediaDirectory = Path.GetDirectoryName(dlg.FileNames[0]);

				MediaOrderItem inserted = null;

				var selectedMedia = OrderListBox.SelectedItem as MediaOrderItem;
				int insertionIndex;
				if (selectedMedia != null)
				{
					insertionIndex = orderList.IndexOf(selectedMedia) + 1;
				}
				else
				{
					insertionIndex = orderList.Count;
				}

				if (dlg.FileNames.Count() > 1)
				{
					foreach (var m in MediaManager.LoadMultipleMediaMetadata(dlg.FileNames.Select(f => new Uri(f))))
						inserted = orderList.Insert(insertionIndex++, m);
				}
				else
				{
					inserted = orderList.Insert(insertionIndex, MediaManager.LoadMediaMetadata(new Uri(dlg.FileName), null));
				}

				OrderListBox.SelectedItem = inserted;

				portfolioModified = true;
			}
		}

		private void NewPortfolio()
		{
			if (portfolioModified && orderList.Count > 0)
			{
				var res = MessageBox.Show(Resource.vMsgSavePortfolioChanges, Resource.vMsgSavePortfolioChangesTitle, MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
				{
					return;
				}
				else if (res == MessageBoxResult.Yes)
				{
					SavePortfolio();

					// it might still be modified if saving was cancelled
					if (portfolioModified)
					{
						return;
					}
				}
			}
			orderList.Clear();
			PortfolioFile = null;
			portfolioModified = false;
		}

		public void OpenPortfolio(string file = null)
		{
			if (portfolioModified && orderList.Count > 0)
			{
				var res = MessageBox.Show(Resource.vMsgSavePortfolioChanges, Resource.vMsgSavePortfolioChangesTitle, MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;
				else if (res == MessageBoxResult.Yes)
					SavePortfolio();
			}

			if (file == null)
			{
				var dlg = new Microsoft.Win32.OpenFileDialog();
				dlg.DefaultExt = ".ppp";
				dlg.Filter = "Powerpraise-Portfolio|*.ppp";
				dlg.InitialDirectory = Properties.Settings.Default.LastPortfolioDirectory;

				if (dlg.ShowDialog() == true)
				{
					file = dlg.FileName;
					Properties.Settings.Default.LastPortfolioDirectory = Path.GetDirectoryName(dlg.FileName);
				}
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
				portfolioModified = false;

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
				portfolioModified = false;
			}
		}

		private void SavePortfolioAs()
		{
			var dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.DefaultExt = ".ppp";
			dlg.Filter = "Powerpraise Portfolio|*.ppp";
			dlg.InitialDirectory = Properties.Settings.Default.LastPortfolioDirectory;

			if (PortfolioFile != null)
			{
				dlg.FileName = PortfolioFile.FullName;
			}

			if (dlg.ShowDialog() == true)
			{
				MediaManager.SavePortfolio(from m in orderList select m.Data, dlg.FileName);
				PortfolioFile = new FileInfo(dlg.FileName);
				Properties.Settings.Default.LastPortfolioDirectory = Path.GetDirectoryName(dlg.FileName);
				portfolioModified = false;
			}
		}

		/// <summary>
		/// Loads a media item from a file and adds it to the current portfolio.
		/// If an item is active, the new item is added after that one, otherwise it is appended
		/// to the end of the portfolio.
		/// </summary>
		/// <param name="uri">The URI to add.</param>
		internal void AddToPortfolio(Uri uri)
		{
			var media = MediaManager.LoadMediaMetadata(uri, null);
			var selectedMedia = OrderListBox.SelectedItem as MediaOrderItem;
			MediaOrderItem inserted;
			if (selectedMedia != null)
			{
				int index = orderList.IndexOf(selectedMedia);
				inserted = orderList.Insert(index + 1, media);
			}
			else
			{
				inserted = orderList.Add(media);
			}

			OrderListBox.SelectedItem = inserted;
		}

		private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.Refresh)
			{
				e.CanExecute = (ActiveMedia != null);
			}
			else if (e.Command == CustomCommands.EditActive)
			{
				e.CanExecute = OrderListBox != null && (ActiveMedia as SongMedia != null);
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
			else if (e.Command == ApplicationCommands.Find)
			{
				Controller.ShowSongList();
			}
			else if (e.Command == CustomCommands.SwitchWindow)
			{
				Controller.ShowEditorWindow();
			}
			else if (e.Command == CustomCommands.EditActive)
			{
				var song = ActiveMedia as SongMedia;
				if (song != null)
				{
					EditorWindow win = Controller.ShowEditorWindow();
					win.LoadOrImport(song.Uri);
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
			else if (e.Command == CustomCommands.ToggleBlackscreen)
			{
				var canShow = Controller.PresentationManager.CurrentPresentation != null;

				if (Controller.PresentationManager.Status == PresentationStatus.Blackscreen && canShow)
				{
					Controller.PresentationManager.Status = PresentationStatus.Show;
				}
				else
				{
					Controller.PresentationManager.Status = PresentationStatus.Blackscreen;
				}
			}
			else if (e.Command == CustomCommands.ShowTestImage)
			{
				orderList.ActiveItem = null;
				Controller.PresentationManager.CurrentPresentation = Controller.PresentationManager.CreatePresentation<TestPresentation>();
				Controller.PresentationManager.Status = PresentationStatus.Show;
			}
			else if (e.Command == CustomCommands.ShowNotification)
			{
				NotificationController.Instance.ShowSettingsWindow();
			}
			else if (e.Command == ApplicationCommands.Help)
			{
				Process.Start("http://wordslive.org/manual/");
			}
			else if (e.Command == CustomCommands.CheckForUpdates)
			{
				Controller.CheckForUpdates(false);
			}
			else if (e.Command == CustomCommands.ShowAboutDialog)
			{
				var dlg = new AboutDialog();
				dlg.Owner = this;
				dlg.ShowDialog();
			}
			else if (e.Command == CustomCommands.CreateSlideshow)
			{
				var dlg = new Microsoft.Win32.SaveFileDialog();
				dlg.DefaultExt = ".show";
				dlg.Filter = "Diashow|*.show"; // TODO: localize
				dlg.InitialDirectory = Properties.Settings.Default.LastMediaDirectory;

				if (dlg.ShowDialog() == true)
				{
					Properties.Settings.Default.LastMediaDirectory = Path.GetDirectoryName(dlg.FileName);
					var slideshowUri = new Uri(dlg.FileName);
					var media = new WordsLive.Images.ImagesMedia(slideshowUri);
					media.CreateSlideshow(new Uri[] {}); // create empty slideshow
					Controller.AddToPortfolio(slideshowUri);
				}
			}
		}

		public void ShowSettingsWindow()
		{
			while (true)
			{
				var win = new SettingsWindow();
				if (this.IsLoaded)
					win.Owner = this;
				MasterOverrideOptions oldOptions = MasterOverrideOptions.CreateFromSettings();
				win.ShowDialog();

				if (win.DialogResult.HasValue && win.DialogResult.Value)
				{
					if (ActiveMedia is SongMedia && !oldOptions.Equals(MasterOverrideOptions.CreateFromSettings()))
					{
						ReloadActiveMedia();
					}

					DataManager.SongTemplate = new FileInfo(Properties.Settings.Default.SongTemplateFile);

					if (Controller.TryUpdateServerSettings())
					{
						break;
					}
					else
					{
						MessageBox.Show(Resource.seMsgInitServerError);
					}
				}
			}
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
							portfolioBackground = SongBackground.Default;

						var win = new ChooseBackgroundWindow(portfolioBackground, true);
						win.Owner = this;
						win.ShowDialog();
						if (win.DialogResult == true)
						{
							portfolioBackground = win.ChosenBackground;
							if (ActiveMedia is SongMedia)
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
						if (ActiveMedia is SongMedia)
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
			var selected = this.OrderListBox.SelectedItems.Cast<MediaOrderItem>().ToArray();
			bool activeSelected = selected.Any() && (selected.Count((item) => item == orderList.ActiveItem) != 0);
			MediaOrderItem boundaryItem;

			if (e.Command == CustomCommands.MoveUp)
			{
				boundaryItem = orderList.Move(selected, -1);
				if (boundaryItem != null)
				{
					OrderListBox.ScrollIntoView(boundaryItem);
					portfolioModified = true;
				}

				// set keyboard focus to last selected element (this is the element that
				// was selected last when using Shift+Arrow key selection, therefore this should keep
				// the selection where it was before) instead of moving focus to list box
				// where it would end up by default with Ctlr+Arrow keys.
				Keyboard.Focus((ListBoxItem)OrderListBox.ItemContainerGenerator.ContainerFromItem(selected.Last()));
			}
			else if (e.Command == CustomCommands.MoveDown)
			{
				boundaryItem = orderList.Move(selected, 1);
				if (boundaryItem != null)
				{
					OrderListBox.ScrollIntoView(boundaryItem);
					portfolioModified = true;
				}

				Keyboard.Focus((ListBoxItem)OrderListBox.ItemContainerGenerator.ContainerFromItem(selected.Last()));
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
					portfolioModified = true;
				}
			}
			else if (e.Command == CustomCommands.Activate)
			{
				orderList.ActiveItem = selected.Last();
			}
			else if (e.Command == NavigationCommands.Refresh)
			{
				foreach (var item in selected)
				{
					orderList.Reload(item);
				}
			}
			else if (e.Command == CustomCommands.OpenInEditor)
			{
				var ed = Controller.ShowEditorWindow();
				foreach (var item in selected)
				{
					var song = item.Data as SongMedia;
					ed.LoadOrImport(song.Uri);
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
			else if (e.Command == CustomCommands.OpenInEditor)
			{
				e.CanExecute = OrderListBox.SelectedItem != null && OrderListBox.SelectedItems.Cast<MediaOrderItem>().All(item => item.Data is SongMedia);
			}
			else
			{
				e.CanExecute = OrderListBox.SelectedItem != null;
			}
		}

		#region Drag & Drop

		int oldIndex;
		Point startPoint;
		InsertionAdorner insertionAdorner;

		private void CreateInsertionAdorner(FrameworkElement targetItemContainer, bool isInFirstHalf)
		{
			if (targetItemContainer != null)
			{
				// Here, I need to get adorner layer from targetItemContainer and not targetItemsControl.
				// This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
				// If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
				var adornerLayer = AdornerLayer.GetAdornerLayer(targetItemContainer);
				this.insertionAdorner = new InsertionAdorner(true, isInFirstHalf, targetItemContainer, adornerLayer);
			}
		}

		private void RemoveInsertionAdorner()
		{
			if (this.insertionAdorner != null)
			{
				this.insertionAdorner.Detach();
				this.insertionAdorner = null;
			}
		}

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
			try
			{
				this.RemoveInsertionAdorner();

				int index = OrderListBox.GetIndexAtPosition(e.GetPosition(OrderListBox));

				FrameworkElement container;
				bool isInFirstHalf = false;

				if (index >= 0)
				{
					container = OrderListBox.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
					isInFirstHalf = e.GetPosition(container).IsInFirstHalf(container, true);
				}
				else
				{
					index = OrderListBox.HasItems ? OrderListBox.Items.Count - 1 : 0;
				}

				// Data comes from list itself
				if (e.Data.GetData(typeof(MediaOrderItem)) != null)
				{

					if (oldIndex < 0 || index == oldIndex)
						return;

					if (index < oldIndex)
						index++;

					if (isInFirstHalf)
						index--;

					MediaOrderItem movedItem = orderList[oldIndex];

					if (index < 0)
						orderList.Move(new MediaOrderItem[] { movedItem }, orderList.Count - oldIndex - 1);
					else
						orderList.Move(new MediaOrderItem[] { movedItem }, index - oldIndex);

					oldIndex = -1;
				}
				// Data comes from song list
				else if (e.Data.GetData(SongDataObject.SongDataFormat) != null)
				{
					if (OrderListBox.HasItems)
						index++;

					if (isInFirstHalf)
						index--;

					SongData data = (SongData)e.Data.GetData(SongDataObject.SongDataFormat);
					Media m = MediaManager.LoadMediaMetadata(data.Uri, null);
					orderList.Insert(index, m);
				}
				// Data comes from explorer
				else if (e.Data.GetData(DataFormats.FileDrop) != null)
				{
					if (OrderListBox.HasItems)
						index++;

					if (isInFirstHalf)
						index--;

					string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

					IEnumerable<Media> result;

					if (files.Length < 1)
						return;

					if (files.Length == 1)
					{
						if (MediaManager.TryLoadPortfolio(files[0], out result))
						{
							// TODO: maybe insert contents at drop position if the portfolio isn't empty?
							Controller.OpenPortfolio(files[0]);
						}
						else
						{
							Media m = MediaManager.LoadMediaMetadata(new Uri(files[0]), null);
							orderList.Insert(index, m);
						}
					}
					else
					{
						foreach (var m in MediaManager.LoadMultipleMediaMetadata(files.Select(f => new Uri(f))))
						{
							orderList.Insert(index++, m);
						}
					}
				}
				else if (e.Data.GetData(typeof(String)) != null)
				{
					string data = (string)e.Data.GetData(typeof(String));
					Uri u = null;
					if (!Uri.TryCreate(data, UriKind.Absolute, out u))
					{
						Uri.TryCreate("http://" + data, UriKind.Absolute, out u);
					}

					if (u != null)
					{
						Media m = MediaManager.LoadMediaMetadata(u, null);
						orderList.Insert(index, m);
					}
				}
			}
			catch (Exception ex)
			{
				Controller.ShowUnhandledException(ex, false);
			}
		}

		private void OrderListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(OrderListBox);
			oldIndex = OrderListBox.GetIndexAtPosition(p);
			if (oldIndex >= 0)
				startPoint = p;
		}

		private void OrderListBox_DragEnterOrOver(object sender, DragEventArgs e)
		{
			int index = OrderListBox.GetIndexAtPosition(e.GetPosition(OrderListBox));
			this.RemoveInsertionAdorner();

			if (index >= 0)
			{
				var container = OrderListBox.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
				this.CreateInsertionAdorner(container, e.GetPosition(container).IsInFirstHalf(container, true));
			}
			else if (OrderListBox.HasItems)
			{
				var container = OrderListBox.ItemContainerGenerator.ContainerFromIndex(OrderListBox.Items.Count - 1) as FrameworkElement;
				this.CreateInsertionAdorner(container, false);
			}

			//e.Effects = DragDropEffects.Move;
		}

		private void OrderListBox_DragLeave(object sender, DragEventArgs e)
		{
			this.RemoveInsertionAdorner();
		}

		#endregion

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.Dispatcher.BeginInvoke((Action)(() =>
			{
				WordsLive.Presentation.Wpf.WpfPresentationWindow.Instance.Owner = this;

				Controller.CheckForUpdates(true);

				Controller.InitializeSettings();

				if (Controller.PresentationAreaSettings.Count(s => s.IsAvailable) > 0)
				{
					Controller.UpdatePresentationAreaFromSettings();
				}
				else
				{
					ShowPresentationAreaSettingsWindow();
				}

				// Update the preview on presentation status changes ...
				Controller.PresentationManager.PropertyChanged += (s, args) =>
				{
					if (args.PropertyName == "Status" || args.PropertyName == "CurrentPresentation")
					{
						UpdatePreview();
					}
				};

				// ... and area changes
				Controller.PresentationManager.Area.WindowSizeChanged += (s, args) =>
				{
					this.previewBorder.Width = Controller.PresentationManager.Area.WindowSize.Width;
					this.previewBorder.Height = Controller.PresentationManager.Area.WindowSize.Height;
				};

				this.previewBorder.Width = Controller.PresentationManager.Area.WindowSize.Width;
				this.previewBorder.Height = Controller.PresentationManager.Area.WindowSize.Height;
			}));
		}

		/// <summary>
		/// This is called from the Application class ...
		/// (1) on startup from the Window.Loaded event
		/// (2) from the single instance controller, when another instance of the program was started
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		public void HandleCommandLineArgs(string[] args)
		{
			this.Dispatcher.BeginInvoke((Action)(() => 
			{
				if (args.Length > 0 && !String.IsNullOrWhiteSpace(args[0]))
				{
					if (args[0].EndsWith(".ppp"))
					{
						OpenPortfolio(args[0]);
					}
					else
					{
						var editor = Controller.ShowEditorWindow();
						editor.LoadOrImport(new Uri(args[0]));
					}
				}
			}));
		}
	}
}