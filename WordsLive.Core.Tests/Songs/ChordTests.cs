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
using WordsLive.Core.Songs.Chords;
using Xunit;

namespace WordsLive.Core.Tests.Songs
{
	// We need to disable parallelization because of the statics in `Chords`
	[CollectionDefinition("ChordTests", DisableParallelization = true)]
	public class ChordTests
	{
		[Theory]
		[InlineData("C", "C")]
		[InlineData("E", "E")]
		[InlineData("F#", "F#")]
		[InlineData("Fis", "F#")]
		[InlineData("Gb", "Gb")]
		[InlineData("G#", "Ab")]
		[InlineData("H", "B")]
		[InlineData("B", "B")]
		[InlineData("Bb", "Bb")]
		public void KeyTest(string key, string expectedNote)
		{
			Chords.GermanNotation = false;
			Chords.LongChordNames = false;
			var k = new Key(key);
			Assert.True(k.IsMajor);
			Assert.Equal(expectedNote, k.ToString());
		}

		[Theory]
		[InlineData("C", "C")]
		[InlineData("E", "E")]
		[InlineData("F#", "F#")]
		[InlineData("Fis", "F#")]
		[InlineData("Gb", "Gb")]
		[InlineData("G#", "Ab")]
		[InlineData("H", "H")]
		[InlineData("B", "B")]
		[InlineData("Bb", "B")]
		public void KeyTestGerman(string key, string expectedNote)
		{
			Chords.GermanNotation = true;
			Chords.LongChordNames = false;
			var k = new Key(key);
			Assert.True(k.IsMajor);
			Assert.Equal(expectedNote, k.ToString());
			Chords.GermanNotation = false;
		}

		[Fact]
		public void KeyTestNonGermanLongNames()
		{
			Chords.GermanNotation = false;
			Chords.LongChordNames = true;
			Assert.Throws<InvalidOperationException>(() => new Key("C").ToString());
			Chords.LongChordNames = false;
		}

		[Theory]
		[InlineData("C", "C")]
		[InlineData("E", "E")]
		[InlineData("F#", "Fis")]
		[InlineData("Fis", "Fis")]
		[InlineData("Gb", "Ges")]
		[InlineData("G#", "As")]
		[InlineData("H", "H")]
		[InlineData("B", "B")]
		[InlineData("Bb", "B")]
		public void KeyTestGermanLongNames(string key, string expectedNote)
		{
			Chords.GermanNotation = true;
			Chords.LongChordNames = true;
			var k = new Key(key);
			Assert.True(k.IsMajor);
			Assert.Equal(expectedNote, k.ToString());
			Chords.LongChordNames = false;
			Chords.GermanNotation = false;
		}
	}
}
