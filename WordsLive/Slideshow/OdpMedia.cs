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
using System.IO;
using WordsLive.Properties;
using WordsLive.Slideshow.Presentation;

namespace WordsLive.Slideshow
{
	public class OdpMedia : SlideshowMedia
	{
		public override SlideshowPresentationTarget Target
		{
			get
			{
				if (Settings.Default.PreferPowerpointForOdp && SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.Powerpoint))
				{
					// prefer PowerPoint if setting is set
					return SlideshowPresentationTarget.Powerpoint;
				}
				else if (SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.Impress))
				{
					// by default use Impress
					return SlideshowPresentationTarget.Impress;
				}
				else if (!Settings.Default.PreferPowerpointForOdp && SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.Powerpoint))
				{
					// use PowerPoint if Impress is not available (and setting is not set)
					return SlideshowPresentationTarget.Powerpoint;
				}
				else
				{
					throw new NotSupportedException("No Supported Target");
				}
			}
		}

		public OdpMedia(Uri uri) : base(uri) { }

		public override void Load()
		{
			if (!Uri.IsFile)
			{
				// TODO: temporarily download file
				throw new NotImplementedException("Loading presentations from remote URIs is not yet implemented.");
			}

			if (!File.Exists(Uri.LocalPath))
			{
				throw new FileNotFoundException();
			}
		}
	}
}
