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
	class SongSlideTests : SongTestsBase
	{
		protected SongPart part;
		protected SongSlide slide;

		public override void Init()
		{
			base.Init();
			part = song.Parts.Single();
			slide = part.Slides.Single();
		}

		[Test]
		public void CloneSlide()
		{
			var clone = slide.Clone();
			Assert.AreEqual("SimpleLine", clone.Text);
			Assert.AreEqual(30, clone.Size);
			clone.Size = 20;
			Assert.AreEqual(30, slide.Size);
		}

		[Test]
		public void ChangeFontSizeUndoRedo()
		{
			Assert.AreEqual(30, slide.Size);
			Assert.AreEqual(30, song.Formatting.MainText.Size);
			slide.Size = 20;
			Assert.AreEqual(20, slide.Size);
			Assert.AreEqual(20, song.Formatting.MainText.Size);
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(30, slide.Size);
			Assert.AreEqual(30, song.Formatting.MainText.Size);
			Redo();
			Assert.AreEqual(20, slide.Size);
			Assert.AreEqual(20, song.Formatting.MainText.Size);
		}

		[Test]
		public void ChangeTextUndoRedo()
		{
			slide.Text = "NewText";
			Assert.AreEqual(1, UndoStackSize);
			Undo();
			Assert.AreEqual(slide.Text, "SimpleLine");
			Redo();
			Assert.AreEqual(slide.Text, "NewText");
		}

		[Test]
		public void ChangeTextNotify()
		{
			bool notified = false;

			slide.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Text")
					notified = true;
			};

			Assert.IsFalse(notified);
			slide.Text = "NewText";
			Assert.IsTrue(notified);
		}

		[Test]
		public void ChangeTextWithChordsNotify()
		{
			bool notified = false;

			slide.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HasChords")
					notified = true;
			};

			Assert.IsFalse(notified);
			slide.Text = "NewText";
			Assert.IsFalse(notified);
			Assert.IsFalse(slide.HasChords);
			slide.Text = "New[C]Text";
			Assert.IsTrue(notified);
			Assert.IsTrue(slide.HasChords);
			Assert.AreEqual("NewText", slide.TextWithoutChords);
		}

		[Test]
		public void ChangeTextMerge()
		{
			slide.Text = "A";
			slide.Translation = "X";
			slide.Text = "AB";
			slide.Text = "ABC";
			slide.Text = "ABCD"; // the last two should be merged with the change above
			Assert.AreEqual(3, UndoStackSize);
			Undo();
			Assert.AreEqual("A", slide.Text);
		}
	}
}
