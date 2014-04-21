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
using System.Linq;
using System.Text;
using WordsLive.Core.Songs;
using Xunit;

namespace WordsLive.Core.Tests.Songs
{
	public class SongSourceTests : SongTestsBase
	{
		[Fact]
		public void SourceParse()
		{
			var source = SongSource.Parse("Book / 1", song);

			Assert.Equal("Book", source.Songbook);
			Assert.Equal(1, source.Number);

			// parsing a source should not influence the undo stack
			Assert.Equal(0, UndoStackSize);
			Assert.Equal(0, RedoStackSize);
		}

		[Fact]
		public void SourceSetSingle()
		{
			song.SetSources(new SongSource[] { });
			Assert.Equal(1, song.Sources.Count);
			Assert.NotNull(song.FirstSource);
			var source = SongSource.Parse("Book / 1", song);
			song.SetSources(new SongSource[] {source});
			Assert.Equal(1, song.Sources.Count);
			Assert.Equal("Book", song.Sources[0].Songbook);
			Assert.Equal(1, song.Sources[0].Number);
			Assert.ReferenceEquals(song.Sources[0], song.FirstSource);
		}

		[Fact]
		public void SourceSetMultiple()
		{
			song.SetSources(new SongSource[] { });
			Assert.Equal(1, song.Sources.Count);
			var sources = new SongSource[] {
				SongSource.Parse("Book / 1", song),
				SongSource.Parse("Book / 2", song),
				SongSource.Parse("Book / 3", song)
			};

			song.SetSources(sources);
			Assert.Equal(3, song.Sources.Count);
			for (int i = 0; i < 3; i++)
			{
				Assert.Equal("Book", song.Sources[i].Songbook);
				Assert.Equal(i + 1, song.Sources[i].Number);
			}
		}

		[Fact]
		public void SourceSetUndoRedo()
		{
			song.SetSources(new SongSource[] { });
			this.ClearUndoRedoStack();
			Assert.Equal(1, song.Sources.Count);
			var sources = new SongSource[] {
				SongSource.Parse("Book / 1", song),
				SongSource.Parse("Book / 2", song),
				SongSource.Parse("Book / 3", song)
			};

			Assert.Equal(0, UndoStackSize);
			song.SetSources(sources);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(1, song.Sources.Count);
			Assert.Equal(String.Empty, song.FirstSource.ToString());
			Redo();
			Assert.Equal(3, song.Sources.Count);
			for (int i = 0; i < 3; i++)
			{
				Assert.Equal("Book", song.Sources[i].Songbook);
				Assert.Equal(i + 1, song.Sources[i].Number);
			}
		}
	}
}
