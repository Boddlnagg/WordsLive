/*
 * WordsLive - worship projection software
 * Copyright (c) 2020 Patrick Reisert
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

using System.Reflection;
using CefSharp;
using CefSharp.Wpf;
using WordsLive.Core;

namespace WordsLive.Cef
{
	public class CefManager
	{
		private static bool initialized = false;
		
		public static void Init()
		{
			// We may be a new window in the same process.
			if (!initialized)
			{
				var settings = new CefSettings
				{
					// see https://github.com/cefsharp/CefSharp/issues/1714 regarding x86
					//BrowserSubprocessPath = System.IO.Path.GetFullPath(@"WordsLive.CefSharp.exe"),
					LogSeverity = LogSeverity.Disable,
					CefCommandLineArgs = { ["disable-gpu-shader-disk-cache"] = "1" },
				};
				settings.RegisterScheme(new CefCustomScheme
				{
					SchemeName = SchemeHandlerFactory.SchemeName,
					SchemeHandlerFactory = new SchemeHandlerFactory()
				});

				CefSharp.Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
				
				initialized = true;
			}
		}

		[Shutdown]
		public static void Shutdown()
		{
			CefSharp.Cef.Shutdown();
		}
	}
}
