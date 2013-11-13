/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
using System.IO;
using System.Linq;
using WordsLive.Core.Songs;

namespace WordsLive.Core.Tests.Songs
{
	public abstract class SongTestsBase
	{
		protected Song song;

		public SongTestsBase()
		{
			// called before each test
			Init();
		}

		public virtual void Init()
		{
			song = new Song(@"TestData\SimpleSong.ppl");
			song.IsUndoEnabled = true;
		}

		protected void ClearUndoRedoStack()
		{
			song.UndoManager.Root.Clear();
		}

		protected void Undo()
		{
			song.UndoManager.Undo();
		}

		protected void Redo()
		{
			song.UndoManager.Redo();
		}

		protected int UndoStackSize
		{
			get
			{
				return song.UndoManager.Root.UndoStack.Count();
			}
		}

		protected int RedoStackSize
		{
			get
			{
				return song.UndoManager.Root.RedoStack.Count();
			}
		}
	}
}
