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
using System.Windows;
using WordsLive.Utils;

namespace WordsLive
{
	/*
	 * TODO:
	 * - Include reveal.js?
	 * - Refactor drag & drop (introduce helper class)
	 * - Add errorlog?
	 * - Remove plugin architecture (formerly used for slideshows)?
	 */

	public partial class App : Application
	{
		private readonly Guid appGuid = new Guid("{CA63A296-1B1C-4675-8B6F-9C95458C7CD1}");

		protected override void OnStartup(StartupEventArgs e)
		{
			//System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
			//System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			WpfSingleInstance.Make(appGuid.ToString());
			MainWindow window = new MainWindow();
			window.Show();
			window.HandleCommandLineArgs(e.Args);

			// FIXME: open portfolio in first instance when a second instance is started with a command line argument
		}
	}
}