/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using WordsLive.Core;
using WordsLive.Slideshow.Presentation;

namespace WordsLive.Slideshow
{
	public class OdpHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".odp" }; }
		}

		public override string Description
		{
			get { return "OpenDocument Presentation"; }
		}

		public override int Test(Uri uri)
		{
			if (!CheckExtension(uri))
				return -1;

			if (!SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.Impress))
				return -1;
			
			return 100;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new OdpMedia(uri);
		}
	}
}
