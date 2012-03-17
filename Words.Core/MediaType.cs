using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Words.Core
{
	public class MediaType
	{
		public string Description { get; private set; }
		public IEnumerable<string> Extensions { get; private set; }
		public Type Type { get; private set; }

		public MediaType(string description, IEnumerable<string> extensions, Type type)
		{
			if (!type.IsSubclassOf(typeof(Media)))
				throw new ArgumentException("type must be subclass of Media");

			Description = description;
			Extensions = extensions;
			Type = type;
		}

		public Media CreateInstance()
		{
			return (Media)Activator.CreateInstance(Type);
		}
	}
}
