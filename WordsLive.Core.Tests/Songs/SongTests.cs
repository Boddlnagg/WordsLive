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

using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using WordsLive.Core.Songs;

namespace WordsLive.Core.Tests.Songs
{
	[TestFixture]
	public class SongTests : SongTestsBase
	{
		[Test]
		public void VerifyLoaded()
		{
			Assert.AreEqual("SimpleTitle", song.SongTitle);
			Assert.AreEqual("SimpleTitle", song.Title);
			Assert.AreEqual("SimpleCategory", song.Category);
			Assert.AreEqual("SimpleCopyright", song.Copyright);
			Assert.AreEqual("English", song.Language);
			Assert.AreEqual("SimpleLine", song.Text);
			Assert.AreEqual("SimpleLine", song.TextWithoutChords);
			Assert.AreEqual(1, song.Sources.Count);
			Assert.AreEqual("SimpleSongbook", song.Sources.First().Songbook);
			Assert.AreEqual(1, song.Parts.Count);

			// parts
			var part = song.Parts.First();
			Assert.AreEqual("SimplePart", part.Name);
			Assert.AreEqual("SimpleLine", part.Text);
			Assert.AreEqual("SimpleLine", part.TextWithoutChords);

			// slides
			Assert.AreEqual(1, part.Slides.Count);
			var slide = part.Slides.First();
			Assert.AreEqual("SimpleLine", slide.Text);
			Assert.AreEqual("SimpleLine", slide.TextWithoutChords);
			Assert.AreEqual(30, slide.Size);
			Assert.AreEqual(0, slide.BackgroundIndex);

			// backgrounds
			Assert.AreEqual(1, song.Backgrounds.Count);
			var bg = song.Backgrounds.First();
			Assert.IsFalse(bg.IsImage);
			Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), bg.Color.ToArgb());
		}

		[Test]
		public void AddRemovePart()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			Assert.AreEqual(2, song.Parts.Count);
			Assert.AreEqual("NewPart", song.Parts[1].Name);
			song.RemovePart(song.Parts[1]);
			Assert.AreEqual(1, song.Parts.Count);
			Assert.AreEqual("SimplePart", song.Parts[0].Name);
		}

		[Test]
		public void AddPartUndoRedo()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			Assert.AreEqual(1, UndoStackSize);
			Undo(); // undo add
			Assert.AreEqual(1, song.Parts.Count);
			Assert.AreEqual("SimplePart", song.Parts[0].Name);
			Redo(); // redo add
			Assert.AreEqual(2, song.Parts.Count);
			Assert.AreEqual("NewPart", song.Parts[1].Name);
		}

		[Test]
		public void RemovePartUndoRedo()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			ClearUndoRedoStack();
			song.RemovePart(song.Parts[0]);
			Assert.AreEqual("NewPart", song.Parts.Single().Name);
			Assert.AreEqual(1, UndoStackSize);
			Undo(); // undo remove
			Assert.AreEqual(2, song.Parts.Count);
			Assert.AreEqual("SimplePart", song.Parts[0].Name);
			Assert.AreEqual("NewPart", song.Parts[1].Name);
			Redo(); // redo remove
			Assert.AreEqual("NewPart", song.Parts.Single().Name);
		}

		[Test]
		public void MovePartUndoRedo()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			ClearUndoRedoStack();
			song.MovePart(song.Parts[1], song.Parts[0]); // move NewPart before SimplePart
			Assert.AreEqual(2, song.Parts.Count);
			Assert.AreEqual("NewPart", song.Parts[0].Name);
			Assert.AreEqual("SimplePart", song.Parts[1].Name);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual("SimplePart", song.Parts[0].Name);
			Assert.AreEqual("NewPart", song.Parts[1].Name);
			Redo();
			Assert.AreEqual(2, song.Parts.Count);
			Assert.AreEqual("NewPart", song.Parts[0].Name);
			Assert.AreEqual("SimplePart", song.Parts[1].Name);
		}

		[Test]
		public void FindPartWithSlide()
		{
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			song.AddPart(new SongPart(song, "NewPart1", new SongSlide[] { slide1 }));
			song.AddPart(new SongPart(song, "NewPart2", new SongSlide[] { slide2 }));
			Assert.AreEqual("NewPart2", song.FindPartWithSlide(slide2).Name);
		}

		[Test]
		public void MoveSlide()
		{
			var part1 = song.Parts.Single();
			var part2 = new SongPart(song, "NewPart", new SongSlide[] { new SongSlide(song) });
			var slide0 = part1.Slides.Single();
			var slide1 = part1.AddSlide();
			song.AddPart(part2);
			ClearUndoRedoStack();

			song.MoveSlide(slide0, part2);
			Assert.AreEqual(1, part1.Slides.Count);
			Assert.AreEqual(2, part2.Slides.Count);
			Assert.AreSame(slide0, part2.Slides[1]);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(2, part1.Slides.Count);
			Assert.AreEqual(1, part2.Slides.Count);
		}

		[Test]
		public void MoveSlideAfter()
		{
			var part1 = song.Parts.Single();
			var slide0 = part1.Slides.Single();
			var slide1 = part1.AddSlide();
			var slide2 = new SongSlide(song);
			var part2 = new SongPart(song, "NewPart", new SongSlide[] { slide2 });
			song.AddPart(part2);
			ClearUndoRedoStack();

			song.MoveSlideAfter(slide0, slide2);
			Assert.AreEqual(1, part1.Slides.Count);
			Assert.AreEqual(2, part2.Slides.Count);
			Assert.AreSame(slide0, part2.Slides[1]);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(2, part1.Slides.Count);
			Assert.AreEqual(1, part2.Slides.Count);
		}

		[Test]
		public void AddBackground()
		{
			Assert.AreEqual(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Black));
			song.AddBackground(new SongBackground(System.Drawing.Color.Red));
			Assert.AreEqual(2, song.Backgrounds.Count);
			song.AddBackground(new SongBackground(System.Drawing.Color.Black)); // black is already there
			Assert.AreEqual(2, song.Backgrounds.Count);
			Assert.AreEqual(2, UndoStackSize);
			Undo();
			Undo();
			Assert.AreEqual(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Black));
		}

		[Test]
		public void CleanBackgrounds()
		{
			song.AddBackground(new SongBackground(System.Drawing.Color.Red));
			ClearUndoRedoStack();

			song.CleanBackgrounds();
			Assert.AreEqual(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Black));
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(2, song.Backgrounds.Count);
			song.Parts[0].Slides[0].BackgroundIndex = 1;
			song.CleanBackgrounds();
			Assert.AreEqual(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Red));
		}

		[Test]
		public void CopyPartUndoRedo()
		{
			var part0 = song.Parts.Single();
			var part1 = song.CopyPart(part0, "PartCopy", part0); // inserts the copy before part0
			Assert.AreEqual(2, song.Parts.Count);
			Assert.AreEqual("PartCopy", song.Parts[0].Name);
			Assert.AreEqual("SimpleLine", song.Parts[0].Text);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual("SimplePart", song.Parts.Single().Name);
			Redo();
			Assert.AreEqual(2, song.Parts.Count);
		}
	}
}
