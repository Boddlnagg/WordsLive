using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MonitoredUndo;
using WordsLive.Core.Songs;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class EditorGrid : Grid, INotifyPropertyChanged
	{
		private SongNodeRoot songNode;
		private bool orderSelected;
		private EditorWindow parent;

		public SongNodeRoot Node
		{
			get
			{
				return songNode;
			}
		}

		public SongNodePart SelectedPart // TODO
		{
			get
			{
				SongNodePart part = null;
				if (StructureTree.SelectedItem is SongNodePart)
				{
					part = StructureTree.SelectedItem as SongNodePart;
				}
				else if (StructureTree.SelectedItem is SongNodeSlide)
				{
					part = songNode.FindPartWithSlide((SongNodeSlide)StructureTree.SelectedItem);
				}
				return part;
			}
		}

		public EditorGrid(Song song, EditorWindow parent)
		{
			InitializeComponent();

			if (song == null)
				throw new ArgumentNullException("song");

			this.parent = parent;

			this.songNode = new SongNodeRoot(song);

			this.StructureTree.DataContext = new SongNode[] { this.songNode };
			var tnp = (Nodes.TreeNodeProvider)FindResource("treeNodeProvider");
			tnp.Song = this.songNode.Song;

			this.StructureTree2.DataContext = new Song[] { song };

			this.OrderListBox.DataContext = this.songNode;

			this.StructureTree.IsEnabled = false;

			this.PreviewControl.FinishedLoading += (sender, args) => InitSelection();
			this.PreviewControl.Song = song;
		}

		private void InitSelection()
		{
			this.StructureTree.IsEnabled = true;

			if (this.StructureTree.IsLoaded)
			{
				this.StructureTree.SetSelectedItem(songNode);
				this.StructureTree.Focus();
			}
		}

		[Obsolete]
		private void AddSlide(SongNodePart item)
		{
			this.StructureTree.SetSelectedItem(item.AddSlide());
		}

		private void AddSlide(SongPart part)
		{
			this.StructureTree.SetSelectedItem(part.AddSlide());
		}

		private void AddPart()
		{
			var res = ShowRenamePartDialog(null);
			if (res.DialogResult.HasValue && res.DialogResult.Value)
			{
				var newPart = songNode.Song.AddPart(res.PartName);
				this.StructureTree.SetSelectedItem(newPart);
			}
		}

		[Obsolete]
		private void RenameSong()
		{
			var res = ShowRenameSongDialog(songNode);
			if (res.DialogResult.HasValue && res.DialogResult.Value)
			{
				songNode.Title = res.SongName;
				// TODO (Editor): ask whether to rename file?
			}
		}

		[Obsolete]
		private void RenamePart(SongNodePart item)
		{
			var res = ShowRenamePartDialog(songNode, item);
			if (res.DialogResult.HasValue && res.DialogResult.Value)
			{
				item.Title = res.PartName;
			}
		}

		[Obsolete]
		private void RemovePart(SongNodePart item)
		{
			this.songNode.RemovePart(item);
		}

		[Obsolete]
		public void RemoveSlide(SongNodeSlide item)
		{
			this.songNode.RemoveSlide(item);
		}

		[Obsolete]
		public void DuplicateSlide(SongNodeSlide item)
		{
			var newSlide = this.songNode.FindPartWithSlide(item).DuplicateSlide(item);
			if (newSlide != null)
				this.StructureTree.SetSelectedItem(newSlide);
		}

		public void SplitSlide(SongNodeSlide item, int splitIndex)
		{
			var newSlide = this.songNode.FindPartWithSlide(item).SplitSlide(item, splitIndex);
			if (newSlide != null)
				this.StructureTree.SetSelectedItem(newSlide);
		}

		#region Drag & Drop
		Point startPoint;
		bool canDrag;
		private int oldIndex;
		private InsertionAdorner insertionAdorner;

		private void OrderListBox_DragEnterOrOver(object sender, DragEventArgs e)
		{
			this.RemoveInsertionAdorner();

			int index = OrderListBox.GetIndexAtPosition(e.GetPosition(OrderListBox));

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

			if (e.Data.GetData(typeof(SongPartWrapper)) is SongPartWrapper)
			{
				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
					e.Effects = DragDropEffects.Copy;
				else
					e.Effects = DragDropEffects.Move;
				e.Handled = true;
			}
			else if (e.Data.GetData(typeof(SongNodePart)) is SongNodePart)
			{
				e.Effects = DragDropEffects.Copy;
				e.Handled = true;
			}
		}

		private void OrderListBox_DragLeave(object sender, DragEventArgs e)
		{
			this.RemoveInsertionAdorner();
		}

		private void OrderListBox_Drop(object sender, DragEventArgs e)
		{
			this.RemoveInsertionAdorner();

			int index = OrderListBox.GetIndexAtPosition(e.GetPosition(OrderListBox));
			bool isInFirstHalf = false;
			if (index >= 0)
			{
				var container = OrderListBox.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
				isInFirstHalf = e.GetPosition(container).IsInFirstHalf(container, true);
			}
			else
			{
				index = OrderListBox.HasItems ? OrderListBox.Items.Count - 1 : 0;
			}

			// Data comes from list itself
			if (e.Data.GetData(typeof(SongPartWrapper)) is SongPartWrapper)
			{
				if (oldIndex < 0)
					return;

				if (index < 0)
					index = OrderListBox.Items.Count;

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
				{
					// copy
					index++;

					if (isInFirstHalf)
						index--;

					songNode.AddPartToOrder(songNode.PartOrder.Skip(oldIndex).First().Content, index);
				}
				else
				{
					// move
					if (index == oldIndex)
						return;

					if (index < oldIndex)
						index++;

					if (isInFirstHalf)
						index--;

					songNode.MovePartInOrder(oldIndex, index);
				}

				OrderListBox.SelectedIndex = index;

				oldIndex = -1;

				e.Handled = true;
			}
			// Data comes from treeview
			else if (e.Data.GetData(typeof(SongNodePart)) is SongNodePart)
			{
				if (index < 0)
					index = OrderListBox.Items.Count;

				if (OrderListBox.HasItems)
					index++;

				if (isInFirstHalf)
					index--;

				songNode.AddPartToOrder((SongNodePart)e.Data.GetData(typeof(SongNodePart)), index);

				e.Handled = true;
			}
		}

		private void OrderListBox_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed)
				oldIndex = -1;

			if (oldIndex < 0)
				return;

			if (e.GetPosition(OrderListBox).ExceedsMinimumDragDistance(startPoint))
			{

				OrderListBox.SelectedIndex = oldIndex;
				SongPartWrapper selectedItem = OrderListBox.SelectedItem as SongPartWrapper;

				if (selectedItem == null)
					return;

				// this will create the drag "rectangle"
				DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
				if (DragDrop.DoDragDrop(this, selectedItem, allowedEffects) != DragDropEffects.None)
				{
					// The item was dropped into a new location,
					// so make it the new selected item. 
					//OrderListBox.SelectedItem = selectedItem;
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

		private void StructureTree2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var tree = (TreeView)sender;
			// TODO: test
			var item = tree.GetItemAtPosition(e.GetPosition(tree));

			if (item != null)
			{
				startPoint = e.GetPosition(tree);
				canDrag = true;
			}
			else
				canDrag = false;
		}

		private void StructureTree2_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			var tree = (TreeView)sender;

			if (e.LeftButton == MouseButtonState.Pressed && canDrag)
			{
				if (e.GetPosition(tree).ExceedsMinimumDragDistance(startPoint))
				{
					if (tree.SelectedItem is SongPart)
					{
						DragDropEffects de = DragDrop.DoDragDrop(tree, tree.SelectedItem as SongPart, DragDropEffects.Move | DragDropEffects.Copy);
					}
					else if (tree.SelectedItem is SongSlide)
					{
						DragDropEffects de = DragDrop.DoDragDrop(tree, tree.SelectedItem as SongSlide, DragDropEffects.Move | DragDropEffects.Copy);
					}
				}
			}
		}

		private void StructureTree2_DragEnterOrOver(object sender, DragEventArgs e)
		{
			var tree = (TreeView)sender;

			if (e.Data.GetData(typeof(SongSlide)) != null)
			{
				var item = tree.GetItemAtPosition(e.GetPosition(tree));
				if (item != null && (item.Header is SongSlide || item.Header is SongPart))
				{
					if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
						e.Effects = DragDropEffects.Copy;
					else
						e.Effects = DragDropEffects.Move;
				}
				else
				{
					e.Effects = DragDropEffects.None;
				}

				e.Handled = true;
			}
			else if (e.Data.GetData(typeof(SongPart)) != null)
			{
				var item = tree.GetItemAtPosition(e.GetPosition(tree));
				if (item != null && (item.Header is SongPart || item.Header is SongSlide))
				{
					if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
						e.Effects = DragDropEffects.Copy;
					else
						e.Effects = DragDropEffects.Move;

					e.Handled = true;
				}
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}
		}

		private void StructureTree2_Drop(object sender, DragEventArgs e)
		{
			var tree = (TreeView)sender;

			if (e.Data.GetData(typeof(SongSlide)) != null)
			{
				ISongElement targetNode = tree.GetItemAtPosition(e.GetPosition(tree)).Header as ISongElement;
				SongSlide dragNode = e.Data.GetData(typeof(SongSlide)) as SongSlide;

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey)) // copy
				{
					if (targetNode is SongSlide)
						songNode.Song.CopySlideAfter(dragNode, targetNode as SongSlide);
					else if (targetNode is SongPart)
						songNode.Song.CopySlide(dragNode, targetNode as SongPart);
				}
				else
				{
					if (targetNode == dragNode)
						return;

					if (songNode.Song.FindPartWithSlide(dragNode).Slides.Count <= 1)
					{
						MessageBox.Show(WordsLive.Resources.Resource.eMsgMoveLastSlideInPart,
							WordsLive.Resources.Resource.dialogError, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					if (targetNode is SongSlide)
						songNode.Song.MoveSlideAfter(dragNode, targetNode as SongSlide);
					else if (targetNode is SongPart)
						songNode.Song.MoveSlide(dragNode, targetNode as SongPart);

					tree.SetSelectedItem(dragNode);
				}
			}
			else if (e.Data.GetData(typeof(SongPart)) != null)
			{
				ISongElement targetNode = tree.GetItemAtPosition(e.GetPosition(tree)).Header as ISongElement;
				SongPart dragNode = e.Data.GetData(typeof(SongPart)) as SongPart;
				SongPart targetPart;
				if (targetNode is SongSlide)
				{
					targetPart = songNode.Song.FindPartWithSlide(targetNode as SongSlide);
				}
				else
				{
					targetPart = (SongPart)targetNode;
				}

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
				{
					// copy part: request name for the copy first
					var res = ShowRenamePartDialog(null);
					if (res.DialogResult.HasValue && res.DialogResult.Value)
					{
						SongPart newPart = songNode.Song.CopyPart(dragNode, res.PartName, targetPart);
						tree.SetSelectedItem(newPart);
					}
				}
				else
				{
					songNode.Song.MovePart(dragNode, targetPart);
					tree.SetSelectedItem(dragNode);
				}
			}
		}

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
		#endregion

		private void StructureTree2_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var tree = (TreeView)sender;

			if (!orderSelected)
				OrderListBox.SelectedItem = null;

			if (tree.SelectedItem is SongPart)
				PreviewControl.Element = ((SongPart)tree.SelectedItem).Slides[0];
			else
				PreviewControl.Element = (ISongElement)tree.SelectedItem;

			// enable/disable spellcheck checkbox
			if (tree.SelectedItem is SongSlide)
				EnableSpellCheckCheckBox.IsEnabled = true;
			else
				EnableSpellCheckCheckBox.IsEnabled = false;

			OnPropertyChanged("SelectedPart");
		}

		private void OrderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (OrderListBox.SelectedItem == null)
				return;

			if (OrderListBox.SelectedIndex == 0)
				PreviewControl.IsFirstSelected = true;
			else
				PreviewControl.IsFirstSelected = false;

			if (OrderListBox.SelectedIndex == OrderListBox.Items.Count - 1)
				PreviewControl.IsLastSelected = true;
			else
				PreviewControl.IsLastSelected = false;

			orderSelected = true;
			var selectedPart = ((SongPartWrapper)OrderListBox.SelectedItem).Content;
			if (selectedPart.Children.Count > 0)
				StructureTree.SetSelectedItem(selectedPart.Children[0]);
			else
				StructureTree.SetSelectedItem(selectedPart);
			orderSelected = false;

			OrderListBox.Focus();
		}

		private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseEventArgs e)
		{
			// Focus the tree and select the item when right-clicked
			TreeView tree = (e.OriginalSource as DependencyObject).FindVisualParent<TreeView>();
			tree.Focus();

			TreeViewItem item = (e.OriginalSource as DependencyObject).FindVisualParent<TreeViewItem, TreeView>();

			if (item != null)
			{
				item.IsSelected = true;
				e.Handled = true;
			}
		}

		[Obsolete]
		private RenameSongWindow ShowRenameSongDialog(SongNodeRoot song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			RenameSongWindow win = new RenameSongWindow(song.Title);
			win.Owner = parent;
			win.ShowDialog();
			return win;
		}

		/// <summary>
		/// Shows a window to prompt for a new name for the song.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <returns>The window.</returns>
		private RenameSongWindow ShowRenameSongDialog(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			RenameSongWindow win = new RenameSongWindow(song.Title);
			win.Owner = parent;
			win.ShowDialog();
			return win;
		}


		[Obsolete]
		private RenamePartWindow ShowRenamePartDialog(SongNodeRoot song, SongNodePart part)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			//RenamePartWindow win = new RenamePartWindow(song, part);
			//win.Owner = parent;
			//win.ShowDialog();
			//return win;
			return null;
		}

		/// <summary>
		/// Show a dialog to prompt for a new name for a part.
		/// </summary>
		/// <param name="part">The part to rename or <c>null</c> to prompt for a name for a new part.</param>
		/// <returns>The window.</returns>
		private RenamePartWindow ShowRenamePartDialog(SongPart part)
		{
			RenamePartWindow win = new RenamePartWindow(songNode.Song, part);
			win.Owner = parent;
			win.ShowDialog();
			return win;
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			SongNode node;

			if (e.Parameter as SongNode != null)
				node = e.Parameter as SongNode;
			else
				node = StructureTree.SelectedItem as SongNode;

			if (e.Command == CustomCommands.Rename)
			{
				if (node is SongNodePart)
					RenamePart(node as SongNodePart);
				else if (node is SongNodeRoot)
					RenameSong();
			}
			else if (e.Command == ApplicationCommands.Delete)
			{
				if (node is SongNodePart)
					RemovePart(node as SongNodePart);
				else if (node is SongNodeSlide)
					RemoveSlide(node as SongNodeSlide);
			}
			else if (e.Command == CustomCommands.Insert)
			{
				if (node is SongNodeRoot)
					AddPart();
				else if (node is SongNodePart)
					AddSlide(node as SongNodePart);
				else if (node is SongNodeSlide)
					AddSlide(songNode.FindPartWithSlide(node as SongNodeSlide));
			}
			else if (e.Command == CustomCommands.AddPart)
			{
				AddPart();
			}
			else if (e.Command == CustomCommands.Duplicate)
			{
				if (node is SongNodeSlide)
					DuplicateSlide(node as SongNodeSlide);
			}
			else if (e.Command == CustomCommands.Split)
			{
				var tb = LogicalTreeHelper.FindLogicalNode(EditBorder.Child, "TextTextBox") as TextBox;
				if (tb != null)
				{
					SplitSlide(node as SongNodeSlide, tb.SelectionStart);
				}
			}
			else if (e.Command == EditingCommands.IncreaseFontSize)
			{
				if (node is SongNodeSlide)
				{
					SongNodeSlide slide = node as SongNodeSlide;
					slide.ChangeFontSize(slide.FontSize + 1);
				}
				else if (node is SongNodeCopyright)
				{
					(node as SongNodeCopyright).FontSize++;
				}
				else if (node is SongNodeSource)
				{
					(node as SongNodeSource).FontSize++;
				}
			}
			else if (e.Command == EditingCommands.DecreaseFontSize)
			{
				if (node is SongNodeSlide)
				{
					SongNodeSlide slide = node as SongNodeSlide;
					slide.ChangeFontSize(slide.FontSize - 1);
				}
				else if (node is SongNodeCopyright)
				{
					(node as SongNodeCopyright).FontSize--;
				}
				else if (node is SongNodeSource)
				{
					(node as SongNodeSource).FontSize--;
				}
			}
		}

		private void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			SongNode node;

			if (e.Parameter as SongNode != null)
				node = e.Parameter as SongNode;
			else
				node = StructureTree.SelectedItem as SongNode;

			if (e.Command == ApplicationCommands.Delete)
			{
				if (node is SongNodePart)
				{
					e.CanExecute = true;
				}
				else if (node is SongNodeSlide)
				{
					// don't delete when there's only one slide left
					e.CanExecute = songNode.FindPartWithSlide(node as SongNodeSlide).Children.Count > 1;
				}
			}
		}

		private void TranslationExpanderExpandedCollapsed(object sender, RoutedEventArgs e)
		{
			// HACK
			Expander exp = (Expander)sender;
			Grid g = exp.FindVisualParent<Grid>();
			if (exp.IsExpanded)
			{
				g.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
				g.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
			}
			else
			{
				g.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
				g.ColumnDefinitions[2].Width = GridLength.Auto;
				((TextBox)g.Children[0]).Focus();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		internal void Cleanup()
		{
			PreviewControl.Cleanup();
		}

		private void OrderListBox_OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Delete)
			{
				int selectedIndex = OrderListBox.SelectedIndex;
				songNode.RemovePartFromOrder(OrderListBox.SelectedIndex);
				if (selectedIndex >= 0)
					OrderListBox.SelectedIndex = selectedIndex < OrderListBox.Items.Count ? selectedIndex : OrderListBox.Items.Count - 1;
			}
			else if (e.Command == CustomCommands.Insert)
			{
				SongNodePart part = SelectedPart;

				if (part != null)
				{
					int index;
					if (OrderListBox.SelectedIndex == -1)
						index = -1;
					else
						index = OrderListBox.SelectedIndex + 1;

					songNode.AddPartToOrder(part, index);

					if (index == -1)
						OrderListBox.SelectedIndex = OrderListBox.Items.Count - 1;
					else
						OrderListBox.SelectedIndex = index;
				}
			}
			else if (e.Command == CustomCommands.MoveUp)
			{
				int moveUpIndex = OrderListBox.SelectedIndex - 1;
				if (moveUpIndex < 0)
					moveUpIndex = 0;
				songNode.MovePartInOrder(OrderListBox.SelectedIndex, moveUpIndex);
				OrderListBox.SelectedIndex = moveUpIndex;
			}
			else if (e.Command == CustomCommands.MoveDown)
			{
				int moveDownIndex = OrderListBox.SelectedIndex + 1;
				if (moveDownIndex > OrderListBox.Items.Count - 1)
					moveDownIndex = OrderListBox.Items.Count - 1;
				songNode.MovePartInOrder(OrderListBox.SelectedIndex, moveDownIndex);
				OrderListBox.SelectedIndex = moveDownIndex;
			}
		}

		private void OrderListBox_OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Delete || e.Command == CustomCommands.MoveUp || e.Command == CustomCommands.MoveDown)
			{
				e.CanExecute = OrderListBox.SelectedItem != null;
			}
			else if (e.Command == CustomCommands.Insert)
			{
				e.CanExecute = SelectedPart != null;
			}
		}

		private void GridCommand_Executed(object sender, ExecutedRoutedEventArgs e) // TODO
		{
			ISongElement node = StructureTree.SelectedItem as ISongElement;

			if (e.Command == CustomCommands.Split)
			{
				// get text cursor position
				var tb = LogicalTreeHelper.FindLogicalNode(EditBorder.Child, "TextTextBox") as TextBox;
				var newSlide = this.songNode.Song.FindPartWithSlide(node as SongSlide).SplitSlide(node as SongSlide, tb.SelectionStart);
			}
			else if (e.Command == EditingCommands.IncreaseFontSize)
			{
				if (node is SongSlide)
				{
					SongSlide slide = node as SongSlide;
					slide.Size++;
				}
				else if (node is Nodes.CopyrightNode)
				{
					var formatting = songNode.Song.Formatting;
					formatting.CopyrightText.Size++;
					songNode.Song.Formatting = formatting;
				}
				else if (node is Nodes.SourceNode)
				{
					var formatting = songNode.Song.Formatting;
					formatting.SourceText.Size++;
					songNode.Song.Formatting = formatting;
				}
			}
			else if (e.Command == EditingCommands.DecreaseFontSize)
			{
				if (node is SongSlide)
				{
					SongSlide slide = node as SongSlide;
					slide.Size--;
				}
				else if (node is Nodes.CopyrightNode)
				{
					var formatting = songNode.Song.Formatting;
					formatting.CopyrightText.Size--;
					songNode.Song.Formatting = formatting;
				}
				else if (node is Nodes.SourceNode)
				{
					var formatting = songNode.Song.Formatting;
					formatting.SourceText.Size--;
					songNode.Song.Formatting = formatting;
				}
			}
		}

		private void StructureTreeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ISongElement node;

			if (e.Parameter != null)
			{
				// called from context menu
				node = e.Parameter as ISongElement;
			}
			else
			{
				// called from button/keyboard
				node = (sender as TreeView).SelectedItem as ISongElement;
			}

			if (node == null)
				throw new InvalidOperationException("node is not a song element");

			if (e.Command == CustomCommands.Rename)
			{
				if (node is SongPart)
				{
					var res = ShowRenamePartDialog(node as SongPart);
					if (res.DialogResult.HasValue && res.DialogResult.Value)
					{
						// apply change
						(node as SongPart).Name = res.PartName;
					}
				}
				else if (node is Song)
				{
					var res = ShowRenameSongDialog(songNode.Song);
					if (res.DialogResult.HasValue && res.DialogResult.Value)
					{
						// apply change
						songNode.Title = res.SongName;
						// TODO (Editor): ask whether to rename file?
					}
				}
			}
			else if (e.Command == ApplicationCommands.Delete)
			{
				if (node is SongPart)
				{
					songNode.Song.RemovePart(node as SongPart);
				}
				else if (node is SongSlide)
				{
					var part = node.Root.FindPartWithSlide(node as SongSlide);
					part.RemoveSlide(node as SongSlide);
				}
			}
			else if (e.Command == CustomCommands.Insert)
			{
				if (node is Song)
				{
					AddPart();
				}
				else if (node is SongPart)
				{
					AddSlide(node as SongPart);
				}
				else if (node is SongSlide)
				{
					AddSlide(songNode.Song.FindPartWithSlide(node as SongSlide));
				}
			}
			else if (e.Command == CustomCommands.AddPart)
			{
				AddPart();
			}
			else if (e.Command == CustomCommands.Duplicate)
			{
				var newSlide = this.songNode.Song.FindPartWithSlide(node as SongSlide).DuplicateSlide(node as SongSlide);
				this.StructureTree.SetSelectedItem(newSlide);
			}
		}

		private void GridCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			ISongElement node = StructureTree2.SelectedItem as ISongElement;

			if (e.Command == EditingCommands.IncreaseFontSize || e.Command == EditingCommands.DecreaseFontSize)
			{
				e.CanExecute = node is SongSlide || node is Nodes.CopyrightNode || node is Nodes.SourceNode;
			}
			else if (e.Command == CustomCommands.Split)
			{
				if (node is SongSlide)
				{
					// get selection length
					var tb = LogicalTreeHelper.FindLogicalNode(EditBorder.Child, "TextTextBox") as TextBox;
					if (tb != null)
						e.CanExecute = (tb.SelectionLength == 0);
				}
			}
		}

		private void StructureTreeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			ISongElement node;

			if (e.Parameter != null)
			{
				// called from context menu
				node = e.Parameter as ISongElement;
			}
			else
			{
				// called from button/keyboard
				node = (sender as TreeView).SelectedItem as ISongElement;
			}

			if (node == null)
				throw new InvalidOperationException("node is not a song element");

			if (e.Command == ApplicationCommands.Delete)
			{
				if (node is SongPart)
				{
					e.CanExecute = true;
				}
				else if (node is SongSlide)
				{
					// don't delete when there's only one slide left
					e.CanExecute = songNode.Song.FindPartWithSlide(node as SongSlide).Slides.Count > 1;
				}
			}
		}
	}
}
