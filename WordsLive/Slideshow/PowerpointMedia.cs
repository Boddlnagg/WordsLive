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
	public class PowerpointMedia : SlideshowMedia
	{
		public override SlideshowPresentationTarget Target
		{
			get
			{
				if (Settings.Default.PreferPowerpointViewerToPowerpoint && SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.PowerpointViewer))
				{
					// prefer PowerPoint Viewer if setting is set
					return SlideshowPresentationTarget.PowerpointViewer;
				}
				else if (SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.Powerpoint))
				{
					// by default prefer PowerPoint
					return SlideshowPresentationTarget.Powerpoint;
				}
				else if (!Settings.Default.PreferPowerpointViewerToPowerpoint && SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.PowerpointViewer))
				{
					// use PowerPoint Viewer if PowerPoint is not available (and setting is not set)
					return SlideshowPresentationTarget.PowerpointViewer;
				}
				else if (SlideshowPresentationFactory.IsTargetAvailable(SlideshowPresentationTarget.Impress))
				{
					// use Impress when nothing else is available
					return SlideshowPresentationTarget.Impress;
				}
				else
				{
					throw new NotSupportedException("No Supported Target");
				}
			} 
		}

		public PowerpointMedia(Uri uri) : base(uri) { }

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
