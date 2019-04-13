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
using System.Linq;
using WordsLive.Core.Songs;
using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace WordsLive.Core.Tests.Songs
{
	public class SongTests : SongTestsBase
	{
		[Fact]
		public void VerifyLoaded()
		{
			Assert.Equal(0, UndoStackSize);
			Assert.Equal(0, RedoStackSize);

			Assert.Equal("SimpleTitle", song.Title);
			Assert.Equal("SimpleCategory", song.Category);
			Assert.Equal("SimpleCopyright", song.Copyright);
			Assert.Equal("English", song.Language);
			Assert.Equal("SimpleLine", song.Text);
			Assert.Equal("SimpleLine", song.TextWithoutChords);
			Assert.Equal(1, song.Sources.Count);
			Assert.Equal("SimpleSongbook", song.Sources.First().Songbook);
			Assert.Equal(1, song.Parts.Count);

			// parts
			var part = song.Parts.First();
			Assert.Equal("SimplePart", part.Name);
			Assert.Equal("SimpleLine", part.Text);
			Assert.Equal("SimpleLine", part.TextWithoutChords);

			// slides
			Assert.Equal(1, part.Slides.Count);
			var slide = part.Slides.First();
			Assert.Equal("SimpleLine", slide.Text);
			Assert.Equal("SimpleLine", slide.TextWithoutChords);
			Assert.Equal(30, slide.Size);
			Assert.Equal(0, slide.BackgroundIndex);

			// backgrounds
			Assert.Equal(1, song.Backgrounds.Count);
			var bg = song.Backgrounds.First();
			Assert.Equal(SongBackgroundType.Color, bg.Type);
			Assert.Equal(System.Drawing.Color.Black.ToArgb(), bg.Color.ToArgb());
		}

		[Fact]
		public void LoadAsync()
		{
			var asyncSong = Song.LoadAsync(@"TestData\SimpleSong.ppl").WaitAndUnwrapException();
			Assert.Equal("SimpleTitle", asyncSong.Title);
		}

		[Fact]
		public void AddRemovePart()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			Assert.Equal(2, song.Parts.Count);
			Assert.Equal("NewPart", song.Parts[1].Name);
			song.RemovePart(song.Parts[1]);
			Assert.Equal(1, song.Parts.Count);
			Assert.Equal("SimplePart", song.Parts[0].Name);
		}

		[Fact]
		public void AddPartUndoRedo()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			Assert.Equal(1, UndoStackSize);
			Undo(); // undo add
			Assert.Equal(1, song.Parts.Count);
			Assert.Equal("SimplePart", song.Parts[0].Name);
			Redo(); // redo add
			Assert.Equal(2, song.Parts.Count);
			Assert.Equal("NewPart", song.Parts[1].Name);
		}

		[Fact]
		public void RemovePartUndoRedo()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			ClearUndoRedoStack();
			song.RemovePart(song.Parts[0]);
			Assert.Equal("NewPart", song.Parts.Single().Name);
			Assert.Equal(1, UndoStackSize);
			Undo(); // undo remove
			Assert.Equal(2, song.Parts.Count);
			Assert.Equal("SimplePart", song.Parts[0].Name);
			Assert.Equal("NewPart", song.Parts[1].Name);
			Redo(); // redo remove
			Assert.Equal("NewPart", song.Parts.Single().Name);
		}

		[Fact]
		public void MovePartUndoRedo()
		{
			song.AddPart(new SongPart(song, "NewPart"));
			ClearUndoRedoStack();
			song.MovePart(song.Parts[1], song.Parts[0]); // move NewPart before SimplePart
			Assert.Equal(2, song.Parts.Count);
			Assert.Equal("NewPart", song.Parts[0].Name);
			Assert.Equal("SimplePart", song.Parts[1].Name);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal("SimplePart", song.Parts[0].Name);
			Assert.Equal("NewPart", song.Parts[1].Name);
			Redo();
			Assert.Equal(2, song.Parts.Count);
			Assert.Equal("NewPart", song.Parts[0].Name);
			Assert.Equal("SimplePart", song.Parts[1].Name);
		}

		[Fact]
		public void FindPartWithSlide()
		{
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			song.AddPart(new SongPart(song, "NewPart1", new SongSlide[] { slide1 }));
			song.AddPart(new SongPart(song, "NewPart2", new SongSlide[] { slide2 }));
			Assert.Equal("NewPart2", song.FindPartWithSlide(slide2).Name);
		}

		[Fact]
		public void MoveSlide()
		{
			var part1 = song.Parts.Single();
			var part2 = new SongPart(song, "NewPart", new SongSlide[] { new SongSlide(song) });
			var slide0 = part1.Slides.Single();
			var slide1 = part1.AddSlide();
			song.AddPart(part2);
			ClearUndoRedoStack();

			song.MoveSlide(slide0, part2);
			Assert.Equal(1, part1.Slides.Count);
			Assert.Equal(2, part2.Slides.Count);
			Assert.Same(slide0, part2.Slides[1]);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(2, part1.Slides.Count);
			Assert.Equal(1, part2.Slides.Count);
		}

		[Fact]
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
			Assert.Equal(1, part1.Slides.Count);
			Assert.Equal(2, part2.Slides.Count);
			Assert.Same(slide0, part2.Slides[1]);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(2, part1.Slides.Count);
			Assert.Equal(1, part2.Slides.Count);
		}

		[Fact]
		public void AddBackground()
		{
			Assert.Equal(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Black));
			song.AddBackground(new SongBackground(System.Drawing.Color.Red));
			Assert.Equal(2, song.Backgrounds.Count);
			song.AddBackground(new SongBackground(System.Drawing.Color.Black)); // black is already there
			Assert.Equal(2, song.Backgrounds.Count);
			Assert.Equal(2, UndoStackSize);
			Undo();
			Undo();
			Assert.Equal(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Black));
		}

		[Fact]
		public void CleanBackgrounds()
		{
			song.AddBackground(new SongBackground(System.Drawing.Color.Red));
			ClearUndoRedoStack();

			song.CleanBackgrounds();
			Assert.Equal(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Black));
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(2, song.Backgrounds.Count);
			song.Parts[0].Slides[0].BackgroundIndex = 1;
			song.CleanBackgrounds();
			Assert.Equal(song.Backgrounds.Single(), new SongBackground(System.Drawing.Color.Red));
		}

		[Fact]
		public void CopyPartUndoRedo()
		{
			var part0 = song.Parts.Single();
			var part1 = song.CopyPart(part0, "PartCopy", part0); // inserts the copy before part0
			Assert.Equal(2, song.Parts.Count);
			Assert.Equal("PartCopy", song.Parts[0].Name);
			Assert.Equal("SimpleLine", song.Parts[0].Text);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal("SimplePart", song.Parts.Single().Name);
			Redo();
			Assert.Equal(2, song.Parts.Count);
		}

		[Fact]
		public void CopySlideUndoRedo()
		{
			var part0 = song.Parts.Single();
			var slide = new SongSlide(song);
			var part1 = new SongPart(song, "NewPart", new SongSlide[] { slide });
			song.AddPart(part1);
			ClearUndoRedoStack();

			song.CopySlide(part0.Slides.Single(), part1);
			Assert.Equal(2, part1.Slides.Count);
			Assert.Equal("SimpleLine", part1.Slides[1].Text);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Same(slide, part1.Slides.Single());
			Redo();
			Assert.Equal(2, part1.Slides.Count);
			Assert.Equal(1, part0.Slides.Count);
		}

		[Fact]
		public void AddPartToOrderUndoRedo()
		{
			var part1 = new SongPart(song, "NewPart1", new SongSlide[] { new SongSlide(song) });
			var part2 = new SongPart(song, "NewPart2", new SongSlide[] { new SongSlide(song) });
			song.AddPart(part1);
			song.AddPart(part2);
			ClearUndoRedoStack();

			song.AddPartToOrder(part1);
			Assert.Equal(2, song.Order.Count);
			Assert.Equal("NewPart1", song.Order[1].Part.Name);
			song.AddPartToOrder(part2, 0);
			Assert.Equal(3, song.Order.Count);
			Assert.Equal("NewPart2", song.Order[0].Part.Name);
			Assert.Equal("NewPart1", song.Order[2].Part.Name);
			Assert.Equal(2, UndoStackSize);
			Undo();
			Assert.Equal("NewPart1", song.Order[1].Part.Name);
			Redo();
			Assert.Equal(3, song.Order.Count);
		}

		[Fact]
		public void MovePartInOrderUndoRedo()
		{
			var part1 = new SongPart(song, "NewPart1", new SongSlide[] { new SongSlide(song) });
			var part2 = new SongPart(song, "NewPart2", new SongSlide[] { new SongSlide(song) });
			song.AddPart(part1);
			song.AddPart(part2);
			var ref1 = song.AddPartToOrder(part1);
			var ref2 = song.AddPartToOrder(part2);
			var ref3 = song.AddPartToOrder(part2);
			ClearUndoRedoStack();

			song.MovePartInOrder(ref2, 0); // move part2 to beginning
			Assert.Same(part2, song.Order[0].Part);
			Assert.Same(ref2, song.Order[0]);
			Assert.Same(ref3, song.Order[3]);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Same(ref2, song.Order[2]);
			Assert.Same(ref3, song.Order[3]);
			Redo();
			Assert.Same(ref2, song.Order[0]);
		}

		[Fact]
		public void RemovePartUndoRedo2()
		{
			var part1 = new SongPart(song, "NewPart1", new SongSlide[] { new SongSlide(song) });
			song.AddPart(part1);
			var reference = song.AddPartToOrder(part1);
			ClearUndoRedoStack();

			Assert.Same(reference, song.Order[1]);
			Assert.Equal(2, song.Order.Count);
			song.RemovePart(part1);
			Assert.Equal(1, song.Parts.Count);
			Assert.Equal(1, song.Order.Count);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Same(reference, song.Order[1]);
			Assert.Equal(2, song.Order.Count);
			Redo();
			Assert.Equal(1, song.Parts.Count);
		}

		[Fact]
		public void RemovePartFromOrder()
		{
			var part0 = song.Parts.Single();
			var part1 = new SongPart(song, "NewPart1", new SongSlide[] { new SongSlide(song) });
			song.AddPart(part1);
			var reference = song.AddPartToOrder(part1);
			ClearUndoRedoStack();
			song.RemovePartFromOrder(song.Order[0]);
			Assert.Equal(1, song.Order.Count);
			Assert.Same(reference, song.Order[0]);
		}

		[Fact]
		public void SetBackgroundUndoRedo()
		{
			var part1 = new SongPart(song, "NewPart1", new SongSlide[] { new SongSlide(song) });
			song.AddPart(part1);
			part1.SetBackground(new SongBackground(System.Drawing.Color.Red));
			ClearUndoRedoStack();

			Assert.Equal(System.Drawing.Color.Black.ToArgb(), song.Parts[0].Slides.Single().Background.Color.ToArgb());
			Assert.Equal(System.Drawing.Color.Red.ToArgb(), song.Parts[1].Slides.Single().Background.Color.ToArgb());
			song.SetBackground(new SongBackground(System.Drawing.Color.Green));
			Assert.Equal(System.Drawing.Color.Green.ToArgb(), song.Backgrounds.Single().Color.ToArgb());
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(System.Drawing.Color.Black.ToArgb(), song.Parts[0].Slides.Single().Background.Color.ToArgb());
			Assert.Equal(System.Drawing.Color.Red.ToArgb(), song.Parts[1].Slides.Single().Background.Color.ToArgb());
			Redo();
			Assert.Equal(System.Drawing.Color.Green.ToArgb(), song.Backgrounds.Single().Color.ToArgb());
		}

		[Fact]
		public void RemoveAllChordsUndoRedo()
		{
			var slide = song.Parts.Single().Slides.Single();
			slide.Text = "Test[Em]Test2[G]";
			ClearUndoRedoStack();

			Core.Songs.Chords.Chords.RemoveAll(song);
			Assert.Equal("TestTest2", slide.Text);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal("Test[Em]Test2[G]", slide.Text);
			Redo();
			Assert.Equal("TestTest2", slide.Text);
		}

		[Fact]
		public void TransposeChordsUndoRedo()
		{
			var slide = song.Parts.Single().Slides.Single();
			slide.Text = "Test[Em]Test2[G]";
			ClearUndoRedoStack();

			Core.Songs.Chords.Chords.Transpose(song, new Core.Songs.Chords.Key("Em"), 5);
			Assert.Equal("Test[Am]Test2[C]", slide.Text);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal("Test[Em]Test2[G]", slide.Text);
			Redo();
			Assert.Equal("Test[Am]Test2[C]", slide.Text);
		}

		[Fact]
		public void CreateSongData()
		{
			song.Title = "Komma, Test\nNeue   Zeile";
			var data = Core.Songs.Storage.SongData.Create(song);
			Assert.Equal("Komma Test Neue Zeile", data.SearchTitle);
		}

		[Fact]
		public void GCCollect()
		{
			song.Title = "Test";
			WeakReference weak = new WeakReference(song);
			song = null;
			GC.Collect();
			Assert.False(weak.IsAlive);
		}

		[Fact]
		public void CheckSingleFontSize()
		{
			Assert.True(song.CheckSingleFontSize());
			Assert.Equal(30, song.Formatting.MainText.Size);
			Assert.Equal(30, song.Parts[0].Slides[0].Size);
		}

		[Fact]
		public void SingleFontSize1()
		{
			var s1 = song.Parts[0].Slides[0];
			var formatting = (SongFormatting)song.Formatting.Clone();
			formatting.SingleFontSize = true;
			song.Formatting = formatting; // applies the formatting -> changes every slide's size
			var s2 = song.Parts[0].AddSlide();
			ClearUndoRedoStack();
			Assert.Equal(30, s1.Size);
			Assert.Equal(30, s2.Size);
			Assert.Equal(30, song.Formatting.MainText.Size);
			s1.Size = 32;
			Assert.Equal(32, s1.Size);
			Assert.Equal(32, s2.Size);
			Assert.Equal(32, song.Formatting.MainText.Size);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(30, s1.Size);
			Assert.Equal(30, s2.Size);
			Redo();
			Assert.Equal(32, s1.Size);
			Assert.Equal(32, s2.Size);
			Assert.Equal(32, song.Formatting.MainText.Size);
			Assert.Equal(1, UndoStackSize);
		}

		[Fact]
		public void SingleFontSize2()
		{
			Assert.False(song.Formatting.SingleFontSize);
			var s1 = song.Parts[0].Slides[0];
			var s2 = song.Parts[0].AddSlide();
			s2.Size = 32;
			Assert.Equal(2, UndoStackSize);
			Assert.False(song.CheckSingleFontSize());
			ClearUndoRedoStack();
			var formatting = (SongFormatting)song.Formatting.Clone();
			formatting.SingleFontSize = true;
			song.Formatting = formatting; // applies the formatting -> changes every slide's size
			Assert.True(song.CheckSingleFontSize());
			Assert.Equal(32, s1.Size);
			Assert.Equal(32, s2.Size);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(30, s1.Size);
			Assert.Equal(32, s2.Size);
			Assert.False(song.Formatting.SingleFontSize);
			Redo();
			Assert.Equal(32, s1.Size);
			Assert.Equal(32, s2.Size);
			Assert.True(song.Formatting.SingleFontSize);
			Assert.Equal(1, UndoStackSize);
		}
	}
}
