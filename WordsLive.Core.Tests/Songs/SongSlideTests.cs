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
	public class SongSlideTests : SongTestsBase
	{
		protected SongPart part;
		protected SongSlide slide;

		public override void Init()
		{
			base.Init();
			part = song.Parts.Single();
			slide = part.Slides.Single();
		}

		[Fact]
		public void CopySlide()
		{
			var copy = slide.Copy();
			Assert.Equal("SimpleLine", copy.Text);
			Assert.Equal(30, copy.Size);
			copy.Size = 20;
			Assert.Equal(30, slide.Size);
		}

		[Fact]
		public void ChangeFontSizeUndoRedo()
		{
			Assert.Equal(30, slide.Size);
			Assert.Equal(30, song.Formatting.MainText.Size);
			slide.Size = 20;
			Assert.Equal(20, slide.Size);
			Assert.Equal(20, song.Formatting.MainText.Size);
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(30, slide.Size);
			Assert.Equal(30, song.Formatting.MainText.Size);
			Redo();
			Assert.Equal(20, slide.Size);
			Assert.Equal(20, song.Formatting.MainText.Size);
		}

		[Fact]
		public void ChangeTextUndoRedo()
		{
			slide.Text = "NewText";
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(slide.Text, "SimpleLine");
			Redo();
			Assert.Equal(slide.Text, "NewText");
		}

		[Fact]
		public void ChangeTextNotify()
		{
			bool notified = false;

			slide.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Text")
					notified = true;
			};

			Assert.False(notified);
			slide.Text = "NewText";
			Assert.True(notified);
		}

		[Fact]
		public void ChangeTextWithChordsNotify()
		{
			bool notified = false;

			slide.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HasChords")
					notified = true;
			};

			Assert.False(notified);
			slide.Text = "NewText";
			Assert.False(notified);
			Assert.False(slide.HasChords);
			slide.Text = "New[C]Text";
			Assert.True(notified);
			Assert.True(slide.HasChords);
			Assert.Equal("NewText", slide.TextWithoutChords);
		}

		[Fact]
		public void ChangeTextMerge()
		{
			slide.Text = "A";
			slide.Translation = "X";
			slide.Text = "AB";
			slide.Text = "ABC";
			slide.Text = "ABCD"; // the last two should be merged with the change above
			Assert.Equal(3, UndoStackSize);
			Undo();
			Assert.Equal("A", slide.Text);
		}

		[Fact]
		public void SetBackgroundUndoRedo()
		{
			Assert.Equal(System.Drawing.Color.Black.ToArgb(), slide.Background.Color.ToArgb());
			var newBg = new SongBackground(System.Drawing.Color.Red);
			slide.SetBackground(newBg);
			Assert.Equal(System.Drawing.Color.Red.ToArgb(), slide.Background.Color.ToArgb());
			Assert.Equal(1, UndoStackSize);
			Undo();
			Assert.Equal(System.Drawing.Color.Black.ToArgb(), slide.Background.Color.ToArgb());
			Redo();
			Assert.Equal(System.Drawing.Color.Red.ToArgb(), slide.Background.Color.ToArgb());
		}
	}
}
