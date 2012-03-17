using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Words.Core
{
	[AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class FileExtensionAttribute : Attribute
	{
		public string Extension { get; set; }

		public FileExtensionAttribute(string extension)
		{
			Extension = extension;
		}
	}
}
