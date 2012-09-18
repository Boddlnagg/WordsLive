using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Words.Core
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ShutdownAttribute : Attribute
	{
	}
}
