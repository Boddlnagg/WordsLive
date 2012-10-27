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

using System.Linq;
using NUnit.Framework;
using WordsLive.Core.Songs;

namespace WordsLive.Core.Tests.Songs
{
	public abstract class SongTestsBase
	{
		protected Song song;

		protected void ClearUndoRedoStack()
		{
			song.UndoRoot.Clear();
		}

		protected void Undo()
		{
			song.UndoRoot.Undo();
		}

		protected void Redo()
		{
			song.UndoRoot.Redo();
		}

		protected int UndoStackSize
		{
			get
			{
				return song.UndoRoot.UndoStack.Count();
			}
		}

		protected int RedoStackSize
		{
			get
			{
				return song.UndoRoot.RedoStack.Count();
			}
		}

		[SetUp]
		public virtual void Init()
		{
			song = new Song(@"TestData\SimpleSong.ppl");
		}
	}
}
