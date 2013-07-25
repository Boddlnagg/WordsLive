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
using NUnit.Framework;
using WordsLive.Core.Songs.Chords;

namespace WordsLive.Core.Tests.Songs
{
	[TestFixture]
	public class ChordTests
	{
		[TearDown]
		public void Reset()
		{
			Chords.LongChordNames = false;
			Chords.GermanNotation = false;
		}

		[Test]
		[TestCase("C", "C")]
		[TestCase("E", "E")]
		[TestCase("F#", "F#")]
		[TestCase("Fis", "F#")]
		[TestCase("Gb", "Gb")]
		[TestCase("G#", "Ab")]
		[TestCase("H", "B")]
		[TestCase("B", "B")]
		[TestCase("Bb", "Bb")]
		public void KeyTest(string key, string expectedNote)
		{
			Chords.GermanNotation = false;
			Chords.LongChordNames = false;
			var k = new Key(key);
			Assert.IsTrue(k.IsMajor);
			Assert.AreEqual(expectedNote, k.ToString());
		}

		[Test]
		[TestCase("C", "C")]
		[TestCase("E", "E")]
		[TestCase("F#", "F#")]
		[TestCase("Fis", "F#")]
		[TestCase("Gb", "Gb")]
		[TestCase("G#", "Ab")]
		[TestCase("H", "H")]
		[TestCase("B", "B")]
		[TestCase("Bb", "B")]
		public void KeyTestGerman(string key, string expectedNote)
		{
			Chords.GermanNotation = true;
			Chords.LongChordNames = false;
			var k = new Key(key);
			Assert.IsTrue(k.IsMajor);
			Assert.AreEqual(expectedNote, k.ToString());
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void KeyTestNonGermanLongNames()
		{
			Chords.GermanNotation = false;
			Chords.LongChordNames = true;
			new Key("C").ToString();
		}

		[Test]
		[TestCase("C", "C")]
		[TestCase("E", "E")]
		[TestCase("F#", "Fis")]
		[TestCase("Fis", "Fis")]
		[TestCase("Gb", "Ges")]
		[TestCase("G#", "As")]
		[TestCase("H", "H")]
		[TestCase("B", "B")]
		[TestCase("Bb", "B")]
		public void KeyTestGermanLongNames(string key, string expectedNote)
		{
			Chords.GermanNotation = true;
			Chords.LongChordNames = true;
			var k = new Key(key);
			Assert.IsTrue(k.IsMajor);
			Assert.AreEqual(expectedNote, k.ToString());
		}
	}
}
