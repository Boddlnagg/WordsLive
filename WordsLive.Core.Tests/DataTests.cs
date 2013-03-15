using System;
using System.IO;
using NUnit.Framework;

namespace WordsLive.Core.Tests
{
	[TestFixture]
	public class DataTests
	{
		[Test]
		public void TempDirectory()
		{
			var dir = new DataManager.TemporaryDirectory();
			DirectoryInfo info = dir.Directory;
			var stream = File.Create(Path.Combine(dir.Directory.FullName, "test.tmp"));
			stream.Close();
			dir = null;
			GC.Collect();
			Assert.IsFalse(info.Exists);
		}
	}
}
