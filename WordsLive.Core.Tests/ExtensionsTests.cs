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
using System.IO;
using Xunit;
using Xunit.Extensions;

namespace WordsLive.Core.Tests
{
	public class ExtensionsTests
	{
		[Fact]
		public void UriGetExtension()
		{
			Assert.Equal(".xml", new Uri("C:\\Foo\\Bar\\Test.xml").GetExtension());
		}

		[Fact]
		public void UriGetExtensionEmpty()
		{
			Assert.Equal("", new Uri("C:\\Foo\\Bar\\Test").GetExtension());
		}

		[Fact]
		public void UriGetExtensionHttp()
		{
			Assert.Equal(".zip", new Uri("http://example.com/Bar/Test.zip").GetExtension());
		}

		[Fact]
		public void UriGetExtensionHttpEmpty()
		{
			Assert.Equal("", new Uri("http://example.com/Bar/Test").GetExtension());
		}

		[Theory]
		[InlineData("C:\\Foo\\Bar\\Test.xml")]
		[InlineData("C:\\Foo\\Bar\\Test")]
		[InlineData("C:\\Foo\\Bar\\")]
		[InlineData("C:\\Foo\\Bar\\Test.xml.zip")]
		[InlineData("C:\\Foo Bar\\Test.xml.zip")]
		[InlineData("C:\\Foo Bar\\Test.XML")]
		public void UriGetExtensionMatchesPath(string path)
		{
			Assert.Equal(Path.GetExtension(path), new Uri(path).GetExtension());
		}

		[Theory]
		[InlineData("C:\\Foo\\Bar\\Test.xml")]
		[InlineData("C:\\Foo\\Bar\\Test")]
		[InlineData("C:\\Foo Bar\\")]
		[InlineData("C:\\Foo\\Bar\\Test.xml.zip")]
		public void FormatLocalMatchesFileInfo(string path)
		{
			Assert.Equal(new FileInfo(path).FullName, new Uri(path).FormatLocal());
		}

		[Theory]
		[InlineData("http://example.com/Bar/")]
		[InlineData("http://www.example.com/Bar/Test")]
		[InlineData("http://localhost/Bar/Test.zip.xml")]
		[InlineData("http://localhost:1234/Bar/Test.xml")]
		public void FormatLocalHttp(string path)
		{
			Assert.Equal(path, new Uri(path).FormatLocal());
		}
	}
}
