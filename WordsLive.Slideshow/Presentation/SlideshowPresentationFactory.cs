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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WordsLive.Slideshow.Presentation
{
	public static class SlideshowPresentationFactory
	{
		private static readonly Type impressPresentationType;
		private static readonly Type powerpointPresentationType;

		static SlideshowPresentationFactory()
		{
			impressPresentationType = TryLoadImpressBridge();
			//powerpointPresentationType = TryLoadPowerpointBridge();
		}

		private static Type TryLoadImpressBridge()
		{
			try
			{
				string startupDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
				var asm = Assembly.LoadFrom(Path.Combine(startupDir, "WordsLive.Slideshow.Impress.Bridge.dll"));
				return asm.GetType("WordsLive.Slideshow.Impress.Bridge.ImpressPresentation");
			}
			catch (FileNotFoundException)
			{
				// this should not happen (the bride dll should always be present)
				throw new DllNotFoundException("WordsLive.Slideshow.Impress.Bridge.dll");
			}
			catch (ReflectionTypeLoadException)
			{
				return null;
			}
		}

		public static bool IsTargetAvailable(SlideshowPresentationTarget target)
		{
			switch (target)
			{
				case SlideshowPresentationTarget.Impress:
					return impressPresentationType != null;
				case SlideshowPresentationTarget.Powerpoint:
					return powerpointPresentationType != null;
				case SlideshowPresentationTarget.PowerpointViewer:
					return PowerpointViewerLib.PowerpointViewerController.IsAvailable;
				default:
					throw new InvalidOperationException("Invalid Slideshow Presentation Target");
			}
		}

		public static ISlideshowPresentation CreatePresentation(SlideshowMedia media)
		{
			switch (media.Target)
			{ 
				case SlideshowPresentationTarget.Impress:
					var impressPres = (ISlideshowPresentation)Controller.PresentationManager.CreatePresentation(impressPresentationType);
					impressPresentationType.GetMethod("Init", new Type[] { typeof(FileInfo) }).Invoke(impressPres, new object[] { new FileInfo(media.Uri.LocalPath) });
					return impressPres;
				case SlideshowPresentationTarget.PowerpointViewer:
					var pptViewPres = Controller.PresentationManager.CreatePresentation<PowerpointViewerPresentation>();
					pptViewPres.File = new FileInfo(media.Uri.LocalPath);
					return pptViewPres;
				case SlideshowPresentationTarget.Powerpoint:
					throw new NotImplementedException();
				default:
					throw new InvalidOperationException("Invalid Slideshow Presentation Target");
			}
		}
	}
}
