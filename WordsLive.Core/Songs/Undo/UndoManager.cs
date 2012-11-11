/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using System.ComponentModel;
using MonitoredUndo;

namespace WordsLive.Core.Songs.Undo
{
	public class UndoManager : INotifyPropertyChanged
	{
		private UndoRoot root;
		private Action setModified;

		/// <summary>
		/// Gets the <see cref="UndoRoot"/> object used for undo/redo actions.
		/// </summary>
		public UndoRoot Root
		{
			get
			{
				return root;
			}
		}

		/// <summary>
		/// Gets a value indicating whether there is a change that can be undone.
		/// </summary>
		public bool CanUndo
		{
			get
			{
				return root.CanUndo;
			}
		}

		/// <summary>
		/// Gets a value indicating whether there is a change that can be redone.
		/// </summary>
		public bool CanRedo
		{
			get
			{
				return root.CanRedo;
			}
		}

		/// <summary>
		/// Undo the latest change.
		/// </summary>
		public void Undo()
		{
			root.Undo();
		}
		
		/// <summary>
		/// Redo the latest change.
		/// </summary>
		public void Redo()
		{
			root.Redo();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UndoManager"/> class.
		/// </summary>
		/// <param name="root">The <see cref="UndoRoot"/> to use for undo/redo actions.</param>
		/// <param name="setModified">An action called whenever a change is made.</param>
		public UndoManager(UndoRoot root, Action setModified)
		{
			this.root = root;
			this.setModified = setModified;
			this.root.UndoStackChanged += UndoStackChanged;
			this.root.RedoStackChanged += RedoStackChanged;
		}

		private void RedoStackChanged(object sender, EventArgs e)
		{
			setModified();
			OnPropertyChanged("CanRedo");
		}

		private void UndoStackChanged(object sender, EventArgs e)
		{
			setModified();
			OnPropertyChanged("CanUndo");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		//private string GetChangesetText(ChangeSet set)
		//{
		//    if (set.Changes.Count() == 0)
		//    {
		//        return set.Description;
		//    }
		//    else
		//    {
		//        string name;
		//        object target = set.Changes.First().Target;
		//        if (target is SongNodeRoot)
		//            name = "Lied";
		//        else if (target is SongNodePart)
		//            name = "Liedteil";
		//        else if (target is SongNodeSlide)
		//            name = "Folie";
		//        else if (target is SongNodeSource)
		//            name = "Quelle";
		//        else if (target is SongNodeMetadata)
		//            name = (target as SongNodeMetadata).Title;
		//        else
		//            throw new InvalidOperationException();

		//        return set.Description + " in " + name;
		//    }
		//}
	}
}
