using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core;

namespace Words
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TargetMediaAttribute : Attribute
	{
		private Type type;

		public Type Type
		{
			get
			{
				return type;
			}
			set
			{
				if (!value.IsSubclassOf(typeof(Media)))
					throw new ArgumentException("Type is not a Media.");

				type = value;
			}
		}

		public TargetMediaAttribute(Type type)
		{
			Type = type;
		}
	}
}
