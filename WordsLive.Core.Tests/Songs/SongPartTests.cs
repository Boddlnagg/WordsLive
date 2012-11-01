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
using System.Linq;
using NUnit.Framework;
using WordsLive.Core.Songs;

namespace WordsLive.Core.Tests.Songs
{
	[TestFixture]
	class SongPartTests : SongTestsBase
	{
		protected SongPart part;

		public override void Init()
		{
			base.Init();
			part = song.Parts.Single();
		}

		[Test]
		public void PartAddSlideUndoRedo()
		{
			int size = song.Formatting.MainText.Size;

			part.AddSlide();
			Assert.AreEqual(2, part.Slides.Count);
			Assert.AreEqual(size, part.Slides[1].Size);
			Assert.IsTrue(String.IsNullOrEmpty(part.Slides[1].Text));
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(1, part.Slides.Count);
			Assert.AreEqual("SimpleLine", part.Slides[0].Text);
			Redo();
			Assert.AreEqual(2, part.Slides.Count);
		}

		[Test]
		public void PartAddSlideExistingUndoRedo()
		{
			var slide = new SongSlide(song) { Text = "NewSlide" };
			ClearUndoRedoStack();

			part.AddSlide(slide);
			Assert.AreEqual(2, part.Slides.Count);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(1, part.Slides.Count);
			Assert.AreEqual("SimpleLine", part.Slides[0].Text);
			Redo();
			Assert.AreEqual(2, part.Slides.Count);
			Assert.AreEqual("NewSlide", part.Slides[1].Text);
		}

		[Test]
		public void PartInsertSlideAfterUndoRedo()
		{
			var slide0 = part.Slides.Single();
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			part.AddSlide(slide1);
			ClearUndoRedoStack();
			part.InsertSlideAfter(slide2, slide0);
			Assert.AreEqual(3, part.Slides.Count);
			Assert.AreSame(slide0, part.Slides[0]);
			Assert.AreSame(slide2, part.Slides[1]);
			Assert.AreSame(slide1, part.Slides[2]);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(2, part.Slides.Count);
			Redo();
			Assert.AreEqual(3, part.Slides.Count);
		}

		[Test]
		public void PartInsertSlideAfter2()
		{
			var slide0 = part.Slides.Single();
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			part.AddSlide(slide1);
			ClearUndoRedoStack();
			part.InsertSlideAfter(slide2, slide1);
			Assert.AreEqual(3, part.Slides.Count);
			Assert.AreSame(slide0, part.Slides[0]);
			Assert.AreSame(slide1, part.Slides[1]);
			Assert.AreSame(slide2, part.Slides[2]);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PartInsertSlideAfter3()
		{
			var slide0 = part.Slides.Single();
			var slide1 = new SongSlide(song);
			var slide2 = new SongSlide(song);
			ClearUndoRedoStack();
			part.InsertSlideAfter(slide2, slide1);
		}

		[Test]
		public void PartRemoveSlideUndoRedo()
		{
			var slide0 = part.Slides.Single();
			var slide1 = part.AddSlide();
			ClearUndoRedoStack();

			part.RemoveSlide(slide0);
			Assert.AreEqual(1, part.Slides.Count);
			Assert.AreSame(slide1, part.Slides.Single());
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(2, part.Slides.Count);
			Assert.AreSame(slide0, part.Slides[0]);
			Redo();
			Assert.AreSame(slide1, part.Slides.Single());
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PartRemoveSlideLast()
		{
			var slide0 = part.Slides.Single();

			part.RemoveSlide(slide0);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PartRemoveSlideDisconnected()
		{
			var slide1 = new SongSlide(song);

			part.RemoveSlide(slide1);
		}

		[Test]
		public void DuplicateSlideUndoRedo()
		{
			part.AddSlide();
			ClearUndoRedoStack();

			part.DuplicateSlide(part.Slides[0]);
			Assert.AreEqual(3, part.Slides.Count);
			Assert.AreEqual("SimpleLine", part.Slides[1].Text);
			part.Slides[1].Text = "SimpleChangedLine";
			Assert.AreEqual("SimpleLine", part.Slides[0].Text);
			Assert.AreEqual(2, UndoStackSize);
			Undo();
			Undo();
			Assert.AreEqual(2, part.Slides.Count);
			Redo();
			Assert.AreEqual(3, part.Slides.Count);
			Assert.AreEqual("SimpleLine", part.Slides[1].Text);
		}

		[Test]
		public void SplitUndoRedo()
		{
			var slide0 = part.Slides.Single();
			var slide1 = part.SplitSlide(slide0, 6);
			Assert.AreEqual(2, part.Slides.Count);
			Assert.AreEqual("Simple", part.Slides[0].Text);
			Assert.AreEqual("Line", part.Slides[1].Text);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreSame(slide0, part.Slides.Single());
			Assert.AreEqual("SimpleLine", part.Slides.Single().Text);
			Redo();
			Assert.AreEqual(2, part.Slides.Count);
		}

		[Test]
		public void SplitMultiline()
		{
			var slide0 = part.Slides.Single();
			slide0.Text = "First\r\nSecond\r\nThird";
			var slide1 = part.SplitSlide(slide0, 5);
			var slide2 = part.SplitSlide(slide1, 6);
			Assert.AreEqual("First", part.Slides[0].Text);
			Assert.AreEqual("Second", part.Slides[1].Text);
			Assert.AreEqual("Third", part.Slides[2].Text);
		}

		[Test]
		public void PartReferenceNotify()
		{
			var partRef = new SongPartReference(part);
			bool notified = false;

			partRef.Part.PropertyChanged += (sender, args) =>
			{
				notified = true;
			};

			Assert.IsFalse(notified);
			part.Name = "NewPartName";
			Assert.AreEqual(new SongPartReference(song, "NewPartName").Part, partRef.Part);
			Assert.IsTrue(notified);
			notified = false;
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.IsTrue(notified);
			notified = false;
			Redo();
			Assert.IsTrue(notified);
			Assert.AreEqual(new SongPartReference(song, "NewPartName").Part, partRef.Part);
		}

		[Test]
		public void PartSetBackgroundUndoRedo()
		{
			song.AddBackground(new SongBackground(System.Drawing.Color.Red));
			part.AddSlide();
			part.Slides[0].BackgroundIndex = 1;
			ClearUndoRedoStack();

			var newBg = new SongBackground(System.Drawing.Color.Green);

			part.SetBackground(newBg);
			Assert.AreEqual(newBg, song.Backgrounds.Single());
			Assert.AreEqual(0, part.Slides[0].BackgroundIndex);
			Assert.AreEqual(0, part.Slides[1].BackgroundIndex);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(song.Backgrounds.Count, 2);
			Assert.AreEqual(1, part.Slides[0].BackgroundIndex);
			Assert.AreEqual(0, part.Slides[1].BackgroundIndex);
		}

		[Test]
		public void CopyPart()
		{
			var copy = part.Copy("PartCopy");
			Assert.AreEqual("PartCopy", copy.Name);
			Assert.AreEqual("SimpleLine", copy.Text);
			part.Slides[0].Text = "ChangedLine";
			Assert.AreEqual("SimpleLine", copy.Text);
		}
	}
}
