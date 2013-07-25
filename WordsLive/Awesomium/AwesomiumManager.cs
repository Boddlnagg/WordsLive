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

using System.Reflection;
using Awesomium.Core;
using WordsLive.Core;

namespace WordsLive.Awesomium
{
	public class AwesomiumManager
	{
		private static bool initialized = false;
		
		// TODO: look at this again and see if it's really necessary (and if it can be simplified)
		// with the new Awesomium version.
		// TODO: use custom-built WebSession for further configuration and add data sources (see below) only once
		public static void Init()
		{
			// We may be a new window in the same process.
			if (!initialized && !WebCore.IsRunning)
			{
				WebCore.CreatedView += WebCore_CreatedView;

				// Setup WebCore with plugins enabled.            
				WebConfig config = new WebConfig
				{
					// !THERE CAN ONLY BE A SINGLE WebCore RUNNING PER PROCESS!
					// We have ensured that our application is single instance,
					// with the use of the WPFSingleInstance utility.
					// We can now safely enable cache and cookies.
					//SaveCacheAndCookies = true,
					// In case our application is installed in ProgramFiles,
					// we wouldn't want the WebCore to attempt to create folders
					// and files in there. We do not have the required privileges.
					// Furthermore, it is important to allow each user account
					// have its own cache and cookies. So, there's no better place
					// than the Application User Data Path.
					/*UserDataPath = My.Application.UserAppDataPath,*/
					
					//EnablePlugins = false, // TODO: make this configurable in case someone wants to use flash ...
					/*HomeURL = Settings.Default.HomeURL,*/
					/*LogPath = My.Application.UserAppDataPath,*/
					LogLevel = LogLevel.None,
					//AcceptLanguageOverride = "de-DE", // TODO: set this to the correct system language (needed for bibleserver)
					ChildProcessPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "WordsLive.Awesomium.exe"),
				};

				//WebCore.Started += (sender, args) => WebCore.BaseDirectory = dataDirectory.FullName;

				// Caution! Do not start the WebCore in window's constructor.
				// This may be a startup window and a synchronization context
				// (necessary for auto-update), may not be available during
				// construction; the Dispatcher may not be running yet 
				// (see App.xaml.cs).
				//
				// Setting the start parameter to false, let's us define
				// configuration settings early enough to be secure, but
				// actually delay the starting of the WebCore until
				// the first WebControl or WebView is created.
				WebCore.Initialize(config, false);

				initialized = true;
			}
		}

		static void WebCore_CreatedView(object sender, CreatedViewEventArgs e)
		{
			e.NewView.WebSession.AddDataSource("WordsLive", new ChainableDataSource(UriMapDataSource.Instance, new ResourceDataSource(Assembly.GetExecutingAssembly())));
			e.NewView.WebSession.AddDataSource("WordsLive.Core", new ChainableDataSource(new ResourceDataSource(Assembly.GetAssembly(typeof(Media)))));
			e.NewView.WebSession.AddDataSource("backgrounds", new ChainableDataSource(new BackgroundDataSource()));
		}

		[Shutdown]
		public static void Shutdown()
		{
			IWebView[] views = new IWebView[WebCore.Views.Count];
			WebCore.Views.CopyTo(views, 0);
			foreach (var v in views)
			{
				v.Dispose();
			}

			if (WebCore.IsRunning && !WebCore.IsShuttingDown)
				WebCore.Shutdown();
		}
	}
}
