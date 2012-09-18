using System;
using Words.Core;
using System.Collections.Generic;
using System.IO;

namespace Words.Core
{
	public abstract class MediaFileHandler
	{
		public abstract IEnumerable<string> Extensions { get; }
		public abstract string Description { get; }
		public abstract Media TryHandle(FileInfo file);

		public virtual Media[] TryHandleMultiple(FileInfo[] files)
		{
			return null;
		}
	}
}
