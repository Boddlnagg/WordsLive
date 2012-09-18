using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WordsLive.Core
{
	public abstract class Media
	{
		public virtual void LoadMetadata(string file)
		{
			File = file;
		}

		public string File { get; private set; }

		public virtual string Title
		{
			get
			{
				return string.IsNullOrEmpty(File) ? null : new FileInfo(File).Name;
			}
		}

		public virtual void Load()
		{	
			
		}
	}
}
