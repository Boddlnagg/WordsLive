using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Words.Core
{
	[AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
	public class MediaTypeAttribute : Attribute
	{
		public IEnumerable<string> Extensions { get; private set; }
		public string Description { get; private set; }

		public MediaTypeAttribute(string description, params string[] extensions)
		{
			Extensions = extensions;
			Description = description;
		}
	}
}
