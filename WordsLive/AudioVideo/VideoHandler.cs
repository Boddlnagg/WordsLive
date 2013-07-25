/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
using System.Linq;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public class VideoHandler : MediaTypeHandler
	{
		private string[] vlcExtensions = new string[] { ".ogv", ".mkv" };
		private string[] extensions = new string[] { ".wmv", ".mp4", ".avi", ".mov", ".m4v" };

		public override IEnumerable<string> Extensions
		{
			get
			{
				if (Properties.Settings.Default.UseVlc && VlcController.IsAvailable)
				{
					return extensions.Concat(vlcExtensions);
				}
				else
				{
					return extensions;
				}
			}
		}

		public override string Description
		{
			get { return "Video-Dateien"; } // TODO: localize
		}

		public override int Test(Uri uri)
		{
			if (CheckExtension(uri))
				return 100;

			if (Properties.Settings.Default.UseVlc && VlcController.IsAvailable)
			{
				if (VideoMedia.IsYouTubeUri(uri))
					return 100;

				if (uri.Scheme == "dshow")
					return 100;
			}

			return -1;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new VideoMedia(uri, options);
		}
	}
}
