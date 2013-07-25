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
using NUnit.Framework;

namespace WordsLive.Core.Tests
{
	[TestFixture]
	public class ExtensionsTests
	{
		[Test]
		public void UriGetExtension()
		{
			Assert.AreEqual(".xml", new Uri("C:\\Foo\\Bar\\Test.xml").GetExtension());
		}

		[Test]
		public void UriGetExtensionEmpty()
		{
			Assert.AreEqual("", new Uri("C:\\Foo\\Bar\\Test").GetExtension());
		}

		[Test]
		public void UriGetExtensionHttp()
		{
			Assert.AreEqual(".zip", new Uri("http://example.com/Bar/Test.zip").GetExtension());
		}

		[Test]
		public void UriGetExtensionHttpEmpty()
		{
			Assert.AreEqual("", new Uri("http://example.com/Bar/Test").GetExtension());
		}

		[Test]
		[TestCase("C:\\Foo\\Bar\\Test.xml")]
		[TestCase("C:\\Foo\\Bar\\Test.")]
		[TestCase("C:\\Foo\\Bar\\")]
		[TestCase("C:\\Foo\\Bar\\Test.xml.zip")]
		[TestCase("C:\\Foo Bar\\Test.xml.zip")]
		[TestCase("C:\\Foo Bar\\Test.XML")]
		public void UriGetExtensionMatchesPath(string path)
		{
			Assert.AreEqual(Path.GetExtension(path), new Uri(path).GetExtension());
		}

		[Test]
		[TestCase("C:\\Foo\\Bar\\Test.xml")]
		[TestCase("C:\\Foo\\Bar\\Test.")]
		[TestCase("C:\\Foo Bar\\")]
		[TestCase("C:\\Foo\\Bar\\Test.xml.zip")]
		public void FormatLocalMatchesFileInfo(string path)
		{
			Assert.AreEqual(new FileInfo(path).FullName, new Uri(path).FormatLocal());
		}

		[Test]
		[TestCase("http://example.com/Bar/")]
		[TestCase("http://www.example.com/Bar/Test")]
		[TestCase("http://localhost/Bar/Test.zip.xml")]
		[TestCase("http://localhost:1234/Bar/Test.xml")]
		public void FormatLocalHttp(string path)
		{
			Assert.AreEqual(path, new Uri(path).FormatLocal());
		}
	}
}
