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

using System.Windows;

namespace WordsLive
{
	/*
	 * TODO:
	 * - Include reveal.js?
	 * - Shortcuts in the menu are shown as "Ctrl" instead of "Strg" in German language
	 * - Maintain correct window z-order using SetWindowPos, especially for ImpressPresentation
	 *   (see http://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx)
	 * - Put WordsLive.Slideshow (+Bridge) under GPLv3 and add COPYING.txt and README.txt
	 *   (with note that PowerpointViewerLib and WordsLive.Slideshow are under GPL) to distribution/installer
	 * - Refactor drag & drop (introduce helper class)
	 * - Save latest directories for portfolios/media separately
	 * - Add errorlog?
	 */

	public partial class App : Application
	{
		public static string StartupPortfolio { get; private set; }

		private void ApplicationStartup(object sender, StartupEventArgs e)
		{
			if (e.Args.Length > 0)
				StartupPortfolio = e.Args[0];

			//Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			//Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		}
	}
}