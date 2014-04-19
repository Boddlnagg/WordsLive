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

namespace WordsLive.Core.Tests.Songs
{
	public class SongPartTests : SongTestsBase
	{
		protected SongPart part;

		public override void Init()
		{
			base.Init();
			part = song.Parts.Single();
		}

		[Fact]
		public void PartAddSlideUndoRedo()
		{
			int size = song.Formatting.MainText.Size;

			part.AddSlide();
			Assert.Equal(2, part.Slides.Count);
			Assert.Equal(size, part.Slides[1].Size);
			Assert.True(String.IsNullOrEmpty(part.Slides[1].Text));
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(1, part.Slides.Count);
			Assert.Equal("SimpleLine", part.Slides[0].Text);
			Redo();
			Assert.Equal(2, part.Slides.Count);
		}

		[Fact]
		public void PartAddSlideExistingUndoRedo()
		{
			var slide = new SongSlide(song) { Text = "NewSlide" };
			ClearUndoRedoStack();

			part.AddSlide(slide);
			Assert.Equal(2, part.Slides.Count);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(1, part.Slides.Count);
			Assert.Equal("SimpleLine", part.Slides[0].Text);
			Redo();
			Assert.Equal(2, part.Slides.Count);
			Assert.Equal("NewSlide", part.Slides[1].Text);
		}

		[Fact]
		public void PartInsertSlideAfterUndoRedo()
		{
			var slide0 = part.Slides.Single();
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			part.AddSlide(slide1);
			ClearUndoRedoStack();
			part.InsertSlideAfter(slide2, slide0);
			Assert.Equal(3, part.Slides.Count);
			Assert.Same(slide0, part.Slides[0]);
			Assert.Same(slide2, part.Slides[1]);
			Assert.Same(slide1, part.Slides[2]);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(2, part.Slides.Count);
			Redo();
			Assert.Equal(3, part.Slides.Count);
		}

		[Fact]
		public void PartInsertSlideAfter2()
		{
			var slide0 = part.Slides.Single();
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			part.AddSlide(slide1);
			ClearUndoRedoStack();
			part.InsertSlideAfter(slide2, slide1);
			Assert.Equal(3, part.Slides.Count);
			Assert.Same(slide0, part.Slides[0]);
			Assert.Same(slide1, part.Slides[1]);
			Assert.Same(slide2, part.Slides[2]);
		}

		[Fact]
		public void PartInsertSlideAfter3()
		{
			var slide0 = part.Slides.Single();
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			ClearUndoRedoStack();
			Assert.Throws<InvalidOperationException>(() => part.InsertSlideAfter(slide2, slide1));
		}

		[Fact]
		public void PartRemoveSlideUndoRedo()
		{
			var slide0 = part.Slides.Single();
			var slide1 = part.AddSlide();
			ClearUndoRedoStack();

			part.RemoveSlide(slide0);
			Assert.Equal(1, part.Slides.Count);
			Assert.Same(slide1, part.Slides.Single());
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(2, part.Slides.Count);
			Assert.Same(slide0, part.Slides[0]);
			Redo();
			Assert.Same(slide1, part.Slides.Single());
		}

		[Fact]
		public void PartRemoveSlideLast()
		{
			var slide0 = part.Slides.Single();

			Assert.Throws<InvalidOperationException>(() => part.RemoveSlide(slide0));
		}

		[Fact]
		public void PartRemoveSlideDisconnected()
		{
			var slide1 = new SongSlide(song);

			Assert.Throws<InvalidOperationException>(() => part.RemoveSlide(slide1));
		}

		[Fact]
		public void DuplicateSlideUndoRedo()
		{
			part.AddSlide();
			ClearUndoRedoStack();

			part.DuplicateSlide(part.Slides[0]);
			Assert.Equal(3, part.Slides.Count);
			Assert.Equal("SimpleLine", part.Slides[1].Text);
			part.Slides[1].Text = "SimpleChangedLine";
			Assert.Equal("SimpleLine", part.Slides[0].Text);
			Assert.Equal(2, UndoStackSize);
			Undo();
			Undo();
			Assert.Equal(2, part.Slides.Count);
			Redo();
			Assert.Equal(3, part.Slides.Count);
			Assert.Equal("SimpleLine", part.Slides[1].Text);
		}

		[Fact]
		public void SplitUndoRedo()
		{
			var slide0 = part.Slides.Single();
			var slide1 = part.SplitSlide(slide0, 6);
			Assert.Equal(2, part.Slides.Count);
			Assert.Equal("Simple", part.Slides[0].Text);
			Assert.Equal("Line", part.Slides[1].Text);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Same(slide0, part.Slides.Single());
			Assert.Equal("SimpleLine", part.Slides.Single().Text);
			Redo();
			Assert.Equal(2, part.Slides.Count);
		}

		[Fact]
		public void SplitMultiline()
		{
			var slide0 = part.Slides.Single();
			slide0.Text = "First\r\nSecond\r\nThird";
			var slide1 = part.SplitSlide(slide0, 5);
			var slide2 = part.SplitSlide(slide1, 6);
			Assert.Equal("First", part.Slides[0].Text);
			Assert.Equal("Second", part.Slides[1].Text);
			Assert.Equal("Third", part.Slides[2].Text);
		}

		[Fact]
		public void PartReferenceNotify()
		{
			var partRef = new SongPartReference(part);
			bool notified = false;

			partRef.Part.PropertyChanged += (sender, args) =>
			{
				notified = true;
			};

			Assert.False(notified);
			part.Name = "NewPartName";
			Assert.Equal(new SongPartReference(song, "NewPartName").Part, partRef.Part);
			Assert.True(notified);
			notified = false;
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.True(notified);
			notified = false;
			Redo();
			Assert.True(notified);
			Assert.Equal(new SongPartReference(song, "NewPartName").Part, partRef.Part);
		}

		[Fact]
		public void PartSetBackgroundUndoRedo()
		{
			song.AddBackground(new SongBackground(System.Drawing.Color.Red));
			part.AddSlide();
			part.Slides[0].BackgroundIndex = 1;
			ClearUndoRedoStack();

			var newBg = new SongBackground(System.Drawing.Color.Green);

			part.SetBackground(newBg);
			Assert.Equal(newBg, song.Backgrounds.Single());
			Assert.Equal(0, part.Slides[0].BackgroundIndex);
			Assert.Equal(0, part.Slides[1].BackgroundIndex);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(song.Backgrounds.Count, 2);
			Assert.Equal(1, part.Slides[0].BackgroundIndex);
			Assert.Equal(0, part.Slides[1].BackgroundIndex);
		}

		[Fact]
		public void CopyPart()
		{
			var copy = part.Copy("PartCopy");
			Assert.Equal("PartCopy", copy.Name);
			Assert.Equal("SimpleLine", copy.Text);
			part.Slides[0].Text = "ChangedLine";
			Assert.Equal("SimpleLine", copy.Text);
		}
	}
}
