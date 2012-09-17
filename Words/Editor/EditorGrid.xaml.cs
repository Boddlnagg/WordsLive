using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MonitoredUndo;
using Words.Core.Songs;
using Words.Utils;

namespace Words.Editor
{
	public partial class EditorGrid : Grid, INotifyPropertyChanged
	{
		private SongNodeRoot songNode;

		public SongNodeRoot Node
		{
			get
			{
				return songNode;
			}
		}

		public SongNodePart SelectedPart
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

		private EditorWindow parent;

		public EditorGrid(Song song, EditorWindow parent)
		{
			InitializeComponent();

			if (song == null)
				throw new ArgumentNullException("song");

			this.parent = parent;
			
			this.songNode = new SongNodeRoot(song);

			this.StructureTree.DataContext = new SongNode[] { this.songNode };
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

		private void AddSlide(SongNodePart item)
		{
			this.StructureTree.SetSelectedItem(item.AddSlide());
		}

		private void AddPart()
		{
			var res = ShowRenamePartDialog(songNode, null);
			if (res.DialogResult.HasValue && res.DialogResult.Value)
			{
				SongNodePart newPart;
				using (new UndoBatch(songNode, "AddPart", false))
				{
					newPart = new SongNodePart(songNode, this.songNode.Song, res.PartName);
					this.songNode.AddPart(newPart);
				}
				this.StructureTree.SetSelectedItem(newPart);
			}
	}

		private void RenameSong()
		{
			var res = ShowRenameSongDialog(songNode);
			if (res.DialogResult.HasValue && res.DialogResult.Value)
			{
				songNode.Title = res.SongName;
				// TODO (Editor): ask whether to rename file?
			}
		}

		private void RenamePart(SongNodePart item)
		{
			var res = ShowRenamePartDialog(songNode, item);
			if (res.DialogResult.HasValue && res.DialogResult.Value)
			{
				item.Title = res.PartName;
			}
		}

		private void RemovePart(SongNodePart item)
		{
			this.songNode.RemovePart(item);
		}

		public void RemoveSlide(SongNodeSlide item)
		{
			this.songNode.RemoveSlide(item);
		}

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

		private void OrderListBox_DragEnterOrOver(object sender, DragEventArgs e)
		{
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

		private void OrderListBox_Drop(object sender, DragEventArgs e)
		{
			int index = OrderListBox.GetIndexAtPosition(e.GetPosition(OrderListBox));

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
					songNode.AddPartToOrder(songNode.PartOrder.Skip(oldIndex).First().Content, index);
				}
				else
				{
					// move
					if (index == oldIndex)
						return;
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

		private void StructureTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource is TextBlock || e.OriginalSource is Image || e.OriginalSource is StackPanel)
			{
				startPoint = e.GetPosition(null);
				canDrag = true;
			}
			else
				canDrag = false;
		}

		private void StructureTree_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && canDrag)
			{
				if (e.GetPosition(null).ExceedsMinimumDragDistance(startPoint))
				{
					if (StructureTree.SelectedItem is SongNodePart)
					{
						DragDropEffects de = DragDrop.DoDragDrop(StructureTree, StructureTree.SelectedItem as SongNodePart, DragDropEffects.Move | DragDropEffects.Copy);
					}
					else if (StructureTree.SelectedItem is SongNodeSlide)
					{
						DragDropEffects de = DragDrop.DoDragDrop(StructureTree, StructureTree.SelectedItem as SongNodeSlide, DragDropEffects.Move | DragDropEffects.Copy);
					}
				}
			}
		}

		private void StructureTree_DragEnterOrOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetData(typeof(SongNodeSlide)) != null)
			{
				var item = StructureTree.GetItemAtPosition(e.GetPosition(StructureTree));
				if (item != null && (item.Header is SongNodeSlide || item.Header is SongNodePart))
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
			else if (e.Data.GetData(typeof(SongNodePart)) != null)
			{
				var item = StructureTree.GetItemAtPosition(e.GetPosition(StructureTree));
				if (item != null && (item.Header is SongNodePart || item.Header is SongNodeSlide))
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
			if (e.Data.GetData(typeof(SongNodeSlide)) != null)
			{
				SongNode targetNode = StructureTree.GetItemAtPosition(e.GetPosition(StructureTree)).Header as SongNode;
				SongNodeSlide dragNode = e.Data.GetData(typeof(SongNodeSlide)) as SongNodeSlide;

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey)) // copy
				{
					if (targetNode is SongNodeSlide)
						Node.CopySlide(dragNode, targetNode as SongNodeSlide);
					else if (targetNode is SongNodePart)
						Node.CopySlide(dragNode, targetNode as SongNodePart);
				}
				else
				{
					if (targetNode == dragNode)
						return;

					if (Node.FindPartWithSlide(dragNode).Children.Count <= 1)
					{
						MessageBox.Show(Words.Resources.Resource.eMsgMoveLastSlideInPart,
							Words.Resources.Resource.dialogError, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					if (targetNode is SongNodeSlide)
						Node.MoveSlide(dragNode, targetNode as SongNodeSlide);
					else if (targetNode is SongNodePart)
						Node.MoveSlide(dragNode, targetNode as SongNodePart);

					StructureTree.SetSelectedItem(dragNode);
				}
			}
			else if (e.Data.GetData(typeof(SongNodePart)) != null)
			{
				SongNode targetNode = StructureTree.GetItemAtPosition(e.GetPosition(StructureTree)).Header as SongNode;
				SongNodePart dragNode = e.Data.GetData(typeof(SongNodePart)) as SongNodePart;
				SongNodePart targetPart;
				if (targetNode is SongNodeSlide)
					targetPart = Node.FindPartWithSlide(targetNode as SongNodeSlide);
				else
					targetPart = (SongNodePart)targetNode;

				if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
				{
					// copy part: request name for the copy first
					var res = ShowRenamePartDialog(songNode, null);
					if (res.DialogResult.HasValue && res.DialogResult.Value)
					{
						SongNodePart newPart;
						using (new UndoBatch(songNode, "CopyPart", false))
						{
							newPart = dragNode.Copy(res.PartName);
							Node.AddPart(newPart);
							Node.MovePart(newPart, targetPart);
						}
						this.StructureTree.SetSelectedItem(newPart);
					}
				}
				else
				{
					Node.MovePart(dragNode, targetPart);
					StructureTree.SetSelectedItem(dragNode);
				}
			}
		}
		#endregion

		bool orderSelected;

		private void StructureTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (!orderSelected)
				OrderListBox.SelectedItem = null;

			if (StructureTree.SelectedItem is SongNodePart)
				PreviewControl.Node = ((SongNodePart)StructureTree.SelectedItem).Children[0];
			else
				PreviewControl.Node = (SongNode)StructureTree.SelectedItem;

			if (StructureTree.SelectedItem is SongNodeSlide)
				EnableSpellCheckCheckBox.IsEnabled = true;
			else
				EnableSpellCheckCheckBox.IsEnabled = false;

			if (StructureTree.SelectedItem is SongNodeRoot)
			{
				EditBorder.Child = null;
				EditHeader.Text = "";
			}
			else if (StructureTree.SelectedItem is SongNodeSlide)
			{
				EditBorder.Child = (Grid)this.Resources["editTextWithTranslation"];
				EditHeader.Text = Words.Resources.Resource.eGridTextHeader;
			}
			else if (StructureTree.SelectedItem is SongNodePart)
			{
				EditBorder.Child = null;
				EditHeader.Text = "";
			}
			else if (StructureTree.SelectedItem is SongNodeLanguage)
			{
				EditBorder.Child = (Grid)this.Resources["editLanguageGrid"];
				EditHeader.Text = ((SongNodeLanguage)StructureTree.SelectedItem).Title;
			}
			else if (StructureTree.SelectedItem is SongNodeSource)
			{
				EditBorder.Child = (Grid)this.Resources["editSourceGrid"];
				EditHeader.Text = ((SongNodeSource)StructureTree.SelectedItem).Title;
			}
			else if (StructureTree.SelectedItem is SongNodeMetadata) // copyright and category
			{
				EditBorder.Child = (TextBox)this.Resources["editTextBox"];
				EditHeader.Text = ((SongNodeMetadata)StructureTree.SelectedItem).Title;
			}
			else
			{
				throw new NotSupportedException();
			}

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
			// Select item when right-clicked
			TreeViewItem item = (e.OriginalSource as DependencyObject).FindVisualParent<TreeViewItem, TreeView>();

			if (item != null)
			{
				item.IsSelected = true;
				e.Handled = true;
			}
		}

		private RenameSongWindow ShowRenameSongDialog(SongNodeRoot song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			RenameSongWindow win = new RenameSongWindow(song.Title);
			win.Owner = parent;
			win.ShowDialog();
			return win;
		}

		private RenamePartWindow ShowRenamePartDialog(SongNodeRoot song, SongNodePart part)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			RenamePartWindow win = new RenamePartWindow(song, part);
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
			else if (e.Command == EditingCommands.IncreaseFontSize || e.Command == EditingCommands.DecreaseFontSize)
			{
				e.CanExecute = node is SongNodeSlide || node is SongNodeCopyright || node is SongNodeSource;
			}
			else if (e.Command == CustomCommands.Split)
			{
				if (node is SongNodeSlide)
				{
					var tb = LogicalTreeHelper.FindLogicalNode(EditBorder.Child, "TextTextBox") as TextBox;
					if (tb != null)
						e.CanExecute = (tb.SelectionLength == 0);
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
	}
}
