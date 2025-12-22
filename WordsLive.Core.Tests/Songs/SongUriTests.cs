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
using WordsLive.Core.Songs.Storage;
using Xunit;

namespace WordsLive.Core.Tests.Songs
{
	public class SongUriTests
	{
		public static TheoryData<string, string, bool> TestData =>
			new TheoryData<string, string, bool>
			{
				{ "test.ppl", "song:///test.ppl", false },
				{ "test and test.ppl", "song:///test and test.ppl", false },
				{ "test+test.ppl", "song:///test%2Btest.ppl", false },
				{ "test&test.ppl", "song:///test%26test.ppl", false },
				{ "test (test).ppl", "song:///test %28test%29.ppl", false },
				{ "test [test].ppl", "song:///test %5Btest%5D.ppl", false },
				{ "#test.ppl", "song:///%23test.ppl", false },
				{ "subfolder/test.ppl", "song:///subfolder/test.ppl", false },
				{ "subfolder\\test.ppl", "song:///subfolder/test.ppl", true },
			};

		[Theory]
		[MemberData(nameof(TestData))]
		public void GetUri(string filename, string uri, bool _)
		{
			Assert.Equal(uri, SongUri.GetUri(filename).ToString());
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public void GetFilename(string filename, string uri, bool oneWayOnly)
		{
			if (oneWayOnly)
			{
				return;
			}
			Assert.Equal(filename, SongUri.GetFilename(new Uri(uri)));
		}
	}
}
