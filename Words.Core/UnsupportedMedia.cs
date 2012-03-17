﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Words.Core
{
	public class UnsupportedMedia : Media
	{
		public override string Title
		{
			get
			{
				return base.Title + " (Format wird nicht unterstützt)";
			}
		}

		protected override bool LoadFromMetadata()
		{
			return false;
		}
	}
}
