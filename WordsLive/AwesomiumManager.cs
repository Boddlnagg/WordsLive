using System.Collections.Generic;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using System;
using Words.Core;
using System.IO;

namespace Words
{
	public class AwesomiumManager
	{
		private static List<IWebView> controls = new List<IWebView>();
		private static bool initialized = false;

		public static void Init()
		{
			// We may be a new window in the same process.
			if (!initialized && !WebCore.IsRunning)
			{
				// Setup WebCore with plugins enabled.            
				WebCoreConfig config = new WebCoreConfig
				{
					// !THERE CAN ONLY BE A SINGLE WebCore RUNNING PER PROCESS!
					// We have ensured that our application is single instance,
					// with the use of the WPFSingleInstance utility.
					// We can now safely enable cache and cookies.
					SaveCacheAndCookies = true,
					// In case our application is installed in ProgramFiles,
					// we wouldn't want the WebCore to attempt to create folders
					// and files in there. We do not have the required privileges.
					// Furthermore, it is important to allow each user account
					// have its own cache and cookies. So, there's no better place
					// than the Application User Data Path.
					/*UserDataPath = My.Application.UserAppDataPath,*/
					EnablePlugins = false, // TODO: make this configurable in case someone wants to use flash ...
					/*HomeURL = Settings.Default.HomeURL,*/
					/*LogPath = My.Application.UserAppDataPath,*/
					LogLevel = LogLevel.Verbose,
					AcceptLanguageOverride = "de-DE", // TODO: set this to the correct system language (needed for bibleserver)
					ChildProcessPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "WordsLive.Awesomium.exe"), // TODO: This doesn't always work ...
				};

				WebCore.Started += (sender, args) => WebCore.BaseDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data");

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

		public static void Register(IWebView web)
		{
			controls.Add(web);
		}

		public static void Close(IWebView web)
		{
			controls.Remove(web);
			web.Close();
		}

		[Shutdown]
		public static void Shutdown()
		{
			foreach (var c in controls)
			{
				c.Close();
			}

			if (WebCore.IsRunning && !WebCore.IsShuttingDown)
				WebCore.Shutdown();
		}
	}
}
