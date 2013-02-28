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
		public void UriGetExtensionMatchesPath(string path)
		{
			Assert.AreEqual(Path.GetExtension(path), new Uri(path).GetExtension());
		}

		[Test]
		[TestCase("C:\\Foo\\Bar\\Test.xml")]
		[TestCase("C:\\Foo\\Bar\\Test.")]
		[TestCase("C:\\Foo\\Bar\\")]
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
