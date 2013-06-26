using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Awesomium.Core;
using Awesomium.Core.Data;
using WordsLive.Core;

namespace WordsLive
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
			e.NewView.WebSession.AddDataSource("WordsLive", new ResourceDataSource(Assembly.GetExecutingAssembly()));
			e.NewView.WebSession.AddDataSource("WordsLive.Core", new ResourceDataSource(Assembly.GetAssembly(typeof(Media))));
			e.NewView.WebSession.AddDataSource("backgrounds", new BackgroundDataSource());
		}

		[Shutdown]
		public static void Shutdown()
		{
			foreach (var v in WebCore.Views)
			{
				v.Dispose();
			}

			if (WebCore.IsRunning && !WebCore.IsShuttingDown)
				WebCore.Shutdown();
		}

		// TODO: move to separate file
		public class ResourceDataSource : DataSource
		{
			private Assembly assembly;
			public ResourceDataSource(Assembly assembly)
			{
				this.assembly = assembly;
			}

			protected override void OnRequest(DataSourceRequest request)
			{
				try
				{
					using (var stream = assembly.GetResourceStream(request.Path))
					{
						byte[] buffer = new byte[stream.Length];
						stream.Read(buffer, 0, (int)stream.Length);
						GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
						IntPtr pointer = pinnedBuffer.AddrOfPinnedObject();
						SendResponse(request, new DataSourceResponse
						{
							Buffer = pointer,
							Size = (uint)stream.Length
						});
						pinnedBuffer.Free();
					}
				}
				catch
				{
					SendRequestFailed(request);
				}
			}
		}

		// TODO: move to separate file
		public class BackgroundDataSource : DataSource
		{
			protected override void OnRequest(DataSourceRequest request)
			{
				try
				{
					var bg = DataManager.Backgrounds.GetFile("/" + request.Path);
					using (WebClient client = new WebClient())
					{
						var bytes = client.DownloadData(bg.Uri);
						GCHandle pinnedBuffer = GCHandle.Alloc(bytes, GCHandleType.Pinned);
						IntPtr pointer = pinnedBuffer.AddrOfPinnedObject();
						SendResponse(request, new DataSourceResponse
						{
							Buffer = pointer,
							Size = (uint)bytes.Length
						});
						pinnedBuffer.Free();
					}
				}
				catch
				{
					SendRequestFailed(request);
				}
			}
		}
	}
}
