using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace Words.Awesomium
{
	class Program
	{
		static void Main(string[] args)
		{
			// Check if our executable is running as a child process.
			if (WebCore.IsChildProcess)
			{
				// Pass control over to Awesomium. This call will return
				// when we are done with using this child process.
				WebCore.ChildProcessMain();
				// Exit this child process.
				return;
			}
		}
	}
}
