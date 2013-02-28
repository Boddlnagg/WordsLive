using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using WordsLive.Core;
using WordsLive.Core.Data;
using WordsLive.Editor;
using WordsLive.MediaOrderList;
using WordsLive.Resources;
using WordsLive.Server;
using WordsLive.Songs;

namespace WordsLive
{
	public class Controller
	{
		// singleton instance
		private static Controller instance = new Controller();

		#region Private fields
		private MainWindow window;

		// these must be instantiated before Init() to be able to bind to them
		private ControlPanelManager controlPanels = new ControlPanelManager();
		private IconProviderManager iconProviders = new IconProviderManager();
		private PresentationManager presentationManager = new PresentationManager();

		private List<Action> shutdownActions = new List<Action>();

		private List<Window> openedWindows = new List<Window>();

		private bool isShuttingDown;
		#endregion

		#region Private methods
		private void Init()
		{
			Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
			window = (MainWindow)Application.Current.MainWindow;
			SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
			DisplaySettingsChanged += (sender, args) => UpdatePresentationAreaFromSettings();

			LoadTypes(Assembly.GetAssembly(typeof(Media))); // WordsLive.Core.dll
			LoadTypes(Assembly.GetExecutingAssembly()); // WordsLive.exe
			LoadTypes(Assembly.GetAssembly(typeof(WordsLive.Presentation.Wpf.WpfPresentationWindow))); // WordsLive.Presentation.Wpf.dll

			string startupDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
			LoadTypes(Assembly.LoadFrom(Path.Combine(startupDir, "WordsLive.Slideshow.dll"))); // TODO (Words): automatically load plugins

			Server = new TestServer(80);
			UpdateServerSettings();

			InitSettings();

			WordsLive.Utils.ImageLoader.Manager.Instance.LoadingImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("/WordsLive;component/Artwork/LoadingAnimation.png", UriKind.Relative));
		}

		void HandleDisplaySettingsChanged(object sender, EventArgs e)
		{
			// TODO: is there a better way then starting a new thread?
			// (we need to wait some time before updating, for else Windows will resize/move the windows again)
			new Thread((ThreadStart) delegate
			{
				Thread.Sleep(1000);
				this.window.Dispatcher.BeginInvoke(new Action(() => OnDisplaySettingsChanged()));
			}).Start();
		}

		public static event EventHandler DisplaySettingsChanged;

		protected static void OnDisplaySettingsChanged()
		{
			if (DisplaySettingsChanged != null)
				DisplaySettingsChanged(null, EventArgs.Empty);
		}

		private void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			if (window.IsLoaded)
			{
				UnhandledExceptionWindow win = new UnhandledExceptionWindow(e.Exception);
				win.Owner = window;
				var result = win.ShowDialog();
				if (result.HasValue && result.Value == false)
					e.Handled = true;
			}
		}

		private void LoadTypes(Assembly assembly)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				foreach (var item in ex.LoaderExceptions)
				{
					// TODO: show warning/error
				}
				types = ex.Types;
			}

			MediaManager.RegisterHandlersFromTypes(types);
			IconProviders.RegisterProvidersFromTypes(types);
			ControlPanels.RegisterPanelsFromTypes(types);

			shutdownActions.AddRange(from type in types
									 from method in type.GetMethods()
									 where (method.IsStatic && method.GetParameters().Length == 0 &&
										 method.GetCustomAttributes(typeof(ShutdownAttribute), true).Cast<ShutdownAttribute>().Count() > 0)
									 select (new Action(() => method.Invoke(null, null))));

			SettingsWindow.Tabs.AddRange(from type in types
										 where type.GetInterfaces().Contains(typeof(ISettingsTab))
										 select type);
		}

		private void InitSettings()
		{
			if (string.IsNullOrEmpty(Properties.Settings.Default.SongsDirectory))
			{
				Properties.Settings.Default.SongsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Powerpraise-Dateien", "Songs"); // TODO: localize?!
			}

			if (string.IsNullOrEmpty(Properties.Settings.Default.BackgroundsDirectory))
			{
				Properties.Settings.Default.BackgroundsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Powerpraise-Dateien", "Backgrounds");
			}

			while (!TryInitDataManager())
			{
				// TODO: this message box is not shown correctly (the first time, when the window is not yet loaded)
				// TODO: show more detailed information about what's wrong (use Exceptions?)
				MessageBox.Show(Resource.seMsgInitDataError);
				window.ShowSettingsWindow();
			}
		}

		private bool TryInitDataManager()
		{
			//init song template file
			if (String.IsNullOrEmpty(Properties.Settings.Default.SongTemplateFile))
				Properties.Settings.Default.SongTemplateFile = Path.Combine("Data", "Standard.ppl");

			var fi = new FileInfo(Properties.Settings.Default.SongTemplateFile);
			if (!fi.Exists)
				return false;
			else
				DataManager.SongTemplate = fi;

			if (Properties.Settings.Default.UseDataServer)
				return DataManager.TryInitUsingServer(Properties.Settings.Default.DataServerAddress, Properties.Settings.Default.DataServerPassword);
			else
				return DataManager.TryInitUsingLocal(Properties.Settings.Default.SongsDirectory, Properties.Settings.Default.BackgroundsDirectory);
		}

		internal static void UpdateServerSettings()
		{
			if (Properties.Settings.Default.EmbeddedServerEnable)
			{
				var port = Properties.Settings.Default.EmbeddedServerPort;
				var pwd = Properties.Settings.Default.EmbeddedServerPassword;
				var enableUI = Properties.Settings.Default.EmbeddedServerEnableUI;

				var settingsChanged = Server.Port != port || Server.Password != pwd;

				// TODO: do we need a restart for change of password?

				if (settingsChanged)
				{
					if (Server.IsRunning)
					{
						// TODO: show warning
						Server.Stop();
					}

					Server.Port = port;
					Server.Password = pwd;

					// TODO: catch exceptions and show meaningful error
					//		 (esp. when port is already used -> show configuration window)
					Server.Start();
				}

				if (Properties.Settings.Default.EmbeddedServerRedirectAll)
				{
					DataManager.EnableRedirect(Controller.Server.CreateSongDataProvider(), Controller.Server.CreateBackgroundStorage());
				}
				else
				{
					DataManager.DisableRedirect();
				}
			}
			else if (Server.IsRunning)
			{
				Server.Stop();
			}
		}

		#endregion

		#region Public (static) members
		public static TestServer Server { get; private set; }

		public static bool IsShuttingDown
		{
			get
			{
				return instance.isShuttingDown;
			}
		}

		public static IEnumerable<PresentationAreaSetting> PresentationAreaSettings
		{
			get
			{
				foreach (var s in Properties.Settings.Default.PresentationAreas)
				{
					yield return PresentationAreaSetting.Import(s);
				}
			}
			set
			{
				Properties.Settings.Default.PresentationAreas.Clear();
				foreach (var setting in value)
				{
					Properties.Settings.Default.PresentationAreas.Add(setting.Export());
				}
			}
		}

		public static ControlPanelManager ControlPanels
		{
			get
			{
				return instance.controlPanels;
			}
		}

		public static IconProviderManager IconProviders
		{
			get
			{
				return instance.iconProviders;
			}
		}

		public static PresentationManager PresentationManager
		{ 
			get
			{
				return instance.presentationManager;
			}
		}

		public static Media ActiveMedia
		{
			get
			{
				return instance.window.ActiveMedia;
			}
		}

		public static IMediaControlPanel CurrentPanel
		{
			get
			{
				return instance.window.CurrentPanel;
			}
			internal set
			{
				instance.window.CurrentPanel = value;
			}
		}

		public static System.Windows.Threading.Dispatcher Dispatcher
		{
			get
			{
				return instance.window.Dispatcher;
			}
		}

		public static void OpenPortfolio(string file)
		{
			instance.window.OpenPortfolio(file);
		}

		public static void AddToPortfolio(string file, IMediaDataProvider provider)
		{
			instance.window.AddToPortfolio(file, provider);
		}

		public static void ShowSongList()
		{
			bool opened = false;
			foreach (Window win in instance.openedWindows)
			{
				if (win is SongListWindow)
				{
					win.Focus();
					opened = true;
					break;
				}
			}

			if (!opened)
			{
				SongListWindow win = new SongListWindow();
				instance.openedWindows.Add(win);
				win.Closed += delegate { instance.openedWindows.Remove(win); };
				win.Show();
			}
		}

		/// <summary>
		/// Shows a window and sets the main window as it's owner.
		/// </summary>
		/// <param name="window">The window to show.</param>
		public static void ShowWindow(Window window)
		{
			instance.openedWindows.Add(window);
			window.Closed += delegate { instance.openedWindows.Remove(window); };
			window.Owner = instance.window;
			window.Show();
		}

		public static EditorWindow ShowEditorWindow()
		{
			foreach (Window win in instance.openedWindows)
			{
				if (win is EditorWindow)
				{
					win.Focus();
					return win as EditorWindow;
				}
			}

			EditorWindow editor = new EditorWindow();
			instance.openedWindows.Add(editor);
			editor.Closed += delegate { instance.openedWindows.Remove(editor); };
			editor.Show();
			return editor;
		}

		public static void FocusMainWindow()
		{
			instance.window.Activate();
			instance.window.Focus();
		}

		public static void ReloadActiveMedia()
		{
			instance.window.ReloadActiveMedia();
		}

		public static void TryActivateNext()
		{
			instance.window.TryActivateNext();
		}

		public static PluginSettingsDictionary PluginSettings
		{
			get
			{
				if (Properties.Settings.Default.PluginSettings == null)
					Properties.Settings.Default.PluginSettings = new PluginSettingsDictionary();

				return Properties.Settings.Default.PluginSettings;
			}
		}
		#endregion

		#region Internal members
		internal static void Initialize()
		{
			instance.Init();
		}

		internal static void UpdatePresentationAreaFromSettings()
		{
			var setting = Controller.PresentationAreaSettings.First(s => s.IsAvailable);
			Controller.PresentationManager.Area.BeginModify();
			Controller.PresentationManager.Area.ScreenIndex = (int)setting.ScreenIndex;
			Controller.PresentationManager.Area.Size = new System.Drawing.Size(setting.Width, setting.Height);
			Controller.PresentationManager.Area.Fullscreen = setting.Fullscreen;
			Controller.PresentationManager.Area.Offset = new System.Drawing.Point(setting.Left, setting.Top);
			Controller.PresentationManager.Area.EndModify();

		}

		internal static void Shutdown()
		{
			instance.isShuttingDown = true;

			// Force all windows to close
			foreach (Window win in Application.Current.Windows)
			{
				if (win != instance.window)
					win.Close();
			}

			PresentationManager.Shutdown();
			//WordsLive.Presentation.Wpf.WpfPresentationWindow.Instance.Close();

			foreach (var action in instance.shutdownActions)
			{
				action();
			}
			Properties.Settings.Default.Save();
		}

		internal static bool TryCloseAllWindows()
		{
			var openedWindowsCopy = instance.openedWindows.ToArray();
			foreach (Window win in openedWindowsCopy)
			{
				win.Close();
				if (instance.openedWindows.Contains(win))
					return false;
			}

			return true;
		}
		#endregion
	}
}
