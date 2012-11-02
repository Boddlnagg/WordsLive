using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WordsLive.Core.Songs;
using WordsLive.Utils;

namespace WordsLive.Editor
{
	public partial class EditorGrid : Grid, INotifyPropertyChanged
	{
		private Song song;
		private bool orderSelected;
		private EditorWindow parent;

		public Song Song
		{
			get
			{
				return song;
			}
		}

		/// <summary>
		/// Gets the part that is currently selected in the structure tree.
		/// </summary>
		public SongPart SelectedPart
		{
			get
			{
				SongPart part = null;
				if (StructureTree.SelectedItem is SongPart)
				{
					part = StructureTree.SelectedItem as SongPart;
				}
				else if (StructureTree.SelectedItem is SongSlide)
				{
					part = song.FindPartWithSlide(StructureTree.SelectedItem as SongSlide);
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

			this.song = song;

			var tnp = (Nodes.TreeNodeProvider)FindResource("treeNodeProvider");
			tnp.Song = song;

			this.StructureTree.IsEnabled = false;

			this.PreviewControl.FinishedLoading += (sender, args) => InitSelection();
			this.PreviewControl.Song = song;
		}

		private void InitSelection()
		{
			this.StructureTree.IsEnabled = true;

			if (this.StructureTree.IsLoaded)
			{
				this.StructureTree.SetSelectedItem(song);
				this.StructureTree.Focus();
			}
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
				var newPart = song.AddPart(res.PartName);
				this.StructureTree.SetSelectedItem(newPart);
			}
		}

		#region Drag & Drop
		Point startPoint;
		bool canDrag;
		private int oldIndex;
		private InsertionAdorner insertionAdorner;

		private void OrderListBox_DragEnterOrOver(object sender, DragEventArgs e)
		{
			var listBox = (ListBox)sender;

			this.RemoveInsertionAdorner();

			int index = listBox.GetIndexAtPosition(e.GetPosition(listBox));

			if (index >= 0)
			{
				var container = listBox.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
				this.CreateInsertionAdorner(container, e.GetPosition(container).IsInFirstHalf(container, true));
			}
			else if (listBox.HasItems)
			{
				var container = listBox.ItemContainerGenerator.ContainerFromIndex(listBox.Items.Count - 1) as FrameworkElement;
				this.CreateInsertionAdorner(container, false);
			}

			if (e.Data.GetData(typeof(SongPartReference)) != null)
			{
				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
					e.Effects = DragDropEffects.Copy;
				else
					e.Effects = DragDropEffects.Move;
				e.Handled = true;
			}
			else if (e.Data.GetData(typeof(SongPart)) != null)
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
			var listBox = (ListBox)sender;

			this.RemoveInsertionAdorner();

			int index = listBox.GetIndexAtPosition(e.GetPosition(listBox));
			bool isInFirstHalf = false;
			if (index >= 0)
			{
				var container = listBox.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
				isInFirstHalf = e.GetPosition(container).IsInFirstHalf(container, true);
			}
			else
			{
				index = listBox.HasItems ? listBox.Items.Count - 1 : 0;
			}

			// Data comes from list itself
			if (e.Data.GetData(typeof(SongPartReference)) != null)
			{
				if (oldIndex < 0)
					return;

				if (index < 0)
					index = listBox.Items.Count;

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
				{
					// copy
					index++;

					if (isInFirstHalf)
						index--;

					song.AddPartToOrder(song.Order[oldIndex].Part, index);
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

					song.MovePartInOrder(song.Order[oldIndex], index);
				}

				listBox.SelectedIndex = index;

				// update undo/redo commands in toolbar
				CommandManager.InvalidateRequerySuggested();

				oldIndex = -1;

				e.Handled = true;
			}
			// Data comes from treeview
			else if (e.Data.GetData(typeof(SongPart)) != null)
			{
				if (index < 0)
					index = listBox.Items.Count;

				if (listBox.HasItems)
					index++;

				if (isInFirstHalf)
					index--;

				song.AddPartToOrder((SongPart)e.Data.GetData(typeof(SongPart)), index);

				e.Handled = true;
			}
		}

		private void OrderListBox_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			var listBox = (ListBox)sender;

			if (e.LeftButton != MouseButtonState.Pressed)
				oldIndex = -1;

			if (oldIndex < 0)
				return;

			if (e.GetPosition(listBox).ExceedsMinimumDragDistance(startPoint))
			{
				listBox.SelectedIndex = oldIndex;
				SongPartReference selectedItem = (SongPartReference)listBox.SelectedItem;

				if (selectedItem == null)
					return;

				// this will create the drag "rectangle"
				DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
				if (DragDrop.DoDragDrop(this, selectedItem, allowedEffects) != DragDropEffects.None)
				{
					// The item was dropped into a new location,
					// so make it the new selected item. 
					//listBox.SelectedItem = selectedItem;
				}
			}
		}

		private void OrderListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var listBox = (ListBox)sender;

			Point p = e.GetPosition(listBox);
			oldIndex = listBox.GetIndexAtPosition(p);
			if (oldIndex >= 0)
				startPoint = p;
		}

		private void StructureTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var tree = (TreeView)sender;
			var item = tree.GetItemAtPosition(e.GetPosition(tree));

			if (item != null)
			{
				startPoint = e.GetPosition(tree);
				canDrag = true;
			}
			else
				canDrag = false;
		}

		private void StructureTree_PreviewMouseMove(object sender, MouseEventArgs e)
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

		private void StructureTree_DragEnterOrOver(object sender, DragEventArgs e)
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

		private void StructureTree_Drop(object sender, DragEventArgs e)
		{
			var tree = (TreeView)sender;

			if (e.Data.GetData(typeof(SongSlide)) != null)
			{
				ISongElement targetNode = tree.GetItemAtPosition(e.GetPosition(tree)).Header as ISongElement;
				SongSlide dragNode = e.Data.GetData(typeof(SongSlide)) as SongSlide;

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey)) // copy
				{
					if (targetNode is SongSlide)
						song.CopySlideAfter(dragNode, targetNode as SongSlide);
					else if (targetNode is SongPart)
						song.CopySlide(dragNode, targetNode as SongPart);
				}
				else
				{
					if (targetNode == dragNode)
						return;

					if (song.FindPartWithSlide(dragNode).Slides.Count <= 1)
					{
						MessageBox.Show(WordsLive.Resources.Resource.eMsgMoveLastSlideInPart,
							WordsLive.Resources.Resource.dialogError, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					if (targetNode is SongSlide)
						song.MoveSlideAfter(dragNode, targetNode as SongSlide);
					else if (targetNode is SongPart)
						song.MoveSlide(dragNode, targetNode as SongPart);

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
					targetPart = song.FindPartWithSlide(targetNode as SongSlide);
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
						SongPart newPart = song.CopyPart(dragNode, res.PartName, targetPart);
						tree.SetSelectedItem(newPart);
					}
				}
				else
				{
					song.MovePart(dragNode, targetPart);
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

		private void StructureTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
			var listBox = (ListBox)sender;

			if (listBox.SelectedItem == null)
				return;

			if (listBox.SelectedIndex == 0)
				PreviewControl.IsFirstSelected = true;
			else
				PreviewControl.IsFirstSelected = false;

			if (listBox.SelectedIndex == listBox.Items.Count - 1)
				PreviewControl.IsLastSelected = true;
			else
				PreviewControl.IsLastSelected = false;

			orderSelected = true;
			var selectedPart = ((SongPartReference)listBox.SelectedItem).Part;
			if (selectedPart.Slides.Count > 0)
				StructureTree.SetSelectedItem(selectedPart.Slides[0]);
			else
				StructureTree.SetSelectedItem(selectedPart);
			orderSelected = false;

			listBox.Focus();
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

		/// <summary>
		/// Show a dialog to prompt for a new name for a part.
		/// </summary>
		/// <param name="part">The part to rename or <c>null</c> to prompt for a name for a new part.</param>
		/// <returns>The window.</returns>
		private RenamePartWindow ShowRenamePartDialog(SongPart part)
		{
			RenamePartWindow win = new RenamePartWindow(song, part);
			win.Owner = parent;
			win.ShowDialog();
			return win;
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

		private void OrderListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var listBox = (ListBox)sender;

			if (e.Command == ApplicationCommands.Delete)
			{
				var oldIndex = listBox.SelectedIndex;

				song.RemovePartFromOrder((SongPartReference)listBox.SelectedItem);
				if (oldIndex >= 0)
					listBox.SelectedIndex = oldIndex < listBox.Items.Count ? oldIndex : listBox.Items.Count - 1;
			}
			else if (e.Command == CustomCommands.Insert)
			{
				SongPart part = SelectedPart;

				if (part != null)
				{
					int index;
					if (listBox.SelectedIndex == -1)
						index = -1;
					else
						index = listBox.SelectedIndex + 1;

					song.AddPartToOrder(part, index);

					if (index == -1)
						listBox.SelectedIndex = listBox.Items.Count - 1;
					else
						listBox.SelectedIndex = index;
				}
			}
			else if (e.Command == CustomCommands.MoveUp)
			{
				int moveUpIndex = listBox.SelectedIndex - 1;
				if (moveUpIndex < 0)
					moveUpIndex = 0;
				song.MovePartInOrder((SongPartReference)listBox.SelectedItem, moveUpIndex);
				listBox.SelectedIndex = moveUpIndex;
			}
			else if (e.Command == CustomCommands.MoveDown)
			{
				int moveDownIndex = listBox.SelectedIndex + 1;
				if (moveDownIndex > listBox.Items.Count - 1)
					moveDownIndex = listBox.Items.Count - 1;
				song.MovePartInOrder((SongPartReference)listBox.SelectedItem, moveDownIndex);
				listBox.SelectedIndex = moveDownIndex;
			}
		}

		private void OrderListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
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

		private void GridCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ISongElement node = StructureTree.SelectedItem as ISongElement;

			if (e.Command == ApplicationCommands.Undo)
			{
				song.UndoManager.Undo();
			}
			else if (e.Command == ApplicationCommands.Redo)
			{
				song.UndoManager.Redo();
			}
			else if (e.Command == CustomCommands.Split)
			{
				// get text cursor position
				var tb = LogicalTreeHelper.FindLogicalNode(EditBorder.Child, "TextTextBox") as TextBox;
				var newSlide = song.FindPartWithSlide(node as SongSlide).SplitSlide(node as SongSlide, tb.SelectionStart);
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
					var formatting = song.Formatting;
					formatting.CopyrightText.Size++;
					song.Formatting = formatting;
				}
				else if (node is Nodes.SourceNode)
				{
					var formatting = song.Formatting;
					formatting.SourceText.Size++;
					song.Formatting = formatting;
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
					var formatting = song.Formatting;
					formatting.CopyrightText.Size--;
					song.Formatting = formatting;
				}
				else if (node is Nodes.SourceNode)
				{
					var formatting = song.Formatting;
					formatting.SourceText.Size--;
					song.Formatting = formatting;
				}
			}

			e.Handled = true;
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
					var res = ShowRenameSongDialog(song);
					if (res.DialogResult.HasValue && res.DialogResult.Value)
					{
						// apply change
						song.SongTitle = res.SongName;
						// TODO (Editor): ask whether to rename file?
					}
				}
			}
			else if (e.Command == ApplicationCommands.Delete)
			{
				if (node is SongPart)
				{
					song.RemovePart(node as SongPart);
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
					AddSlide(song.FindPartWithSlide(node as SongSlide));
				}
			}
			else if (e.Command == CustomCommands.AddPart)
			{
				AddPart();
			}
			else if (e.Command == CustomCommands.Duplicate)
			{
				var newSlide = song.FindPartWithSlide(node as SongSlide).DuplicateSlide(node as SongSlide);
				this.StructureTree.SetSelectedItem(newSlide);
			}
		}

		private void GridCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			ISongElement node = StructureTree.SelectedItem as ISongElement;

			if (e.Command == ApplicationCommands.Undo)
			{
				e.CanExecute = song.UndoManager.CanUndo;
			}
			else if (e.Command == ApplicationCommands.Redo)
			{
				e.CanExecute = song.UndoManager.CanRedo;
			}
			else if (e.Command == EditingCommands.IncreaseFontSize || e.Command == EditingCommands.DecreaseFontSize)
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

			e.Handled = true;
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
					e.CanExecute = song.FindPartWithSlide(node as SongSlide).Slides.Count > 1;
				}
			}
		}
	}
}
