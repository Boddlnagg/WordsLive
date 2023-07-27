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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using WordsLive.Core;
using WordsLive.Editor;
using WordsLive.MediaOrderList;
using WordsLive.Resources;
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
		private bool isFirstStart = false;
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

			WordsLive.Utils.ImageLoader.Manager.Instance.LoadingImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("/WordsLive;component/Artwork/LoadingAnimation.png", UriKind.Relative));

			UpgradeSettings();
		}

		void HandleDisplaySettingsChanged(object sender, EventArgs e)
		{
			// TODO: is there a better way than starting a new thread?
			// TODO: use Task.Factory.StartNew and Task.Delay()
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
			if (window != null && window.IsLoaded)
			{
				e.Handled = ShowUnhandledException(e.Exception, true);
			}
		}

		public static bool ShowUnhandledException(Exception e, bool canAbort = true)
		{
			UnhandledExceptionWindow win = new UnhandledExceptionWindow(e, canAbort);
			win.Owner = instance.window;
			var result = win.ShowDialog();
			return (result.HasValue && result.Value == false);
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

		private static void UpgradeSettings()
		{
			var a = Assembly.GetExecutingAssembly();
			Version appVersion = a.GetName().Version;
			string appVersionString = appVersion.ToString();
			bool modified = false;
			
			if (Properties.Settings.Default.ApplicationVersion != appVersionString)
			{
				Properties.Settings.Default.Upgrade();

				if (String.IsNullOrEmpty(Properties.Settings.Default.ApplicationVersion))
				{
					instance.isFirstStart = true;
				}

				Properties.Settings.Default.ApplicationVersion = appVersionString;
				Properties.Settings.Default.NoUpdateVersion = appVersionString;
				modified = true;
			}

			if (String.IsNullOrEmpty(Properties.Settings.Default.NoUpdateVersion))
			{
				Properties.Settings.Default.NoUpdateVersion = appVersionString;
				modified = true;
			}

			if (modified)
			{
				Properties.Settings.Default.Save();
			}
		}

		internal static void InitializeSettings()
		{
			var appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

			if (instance.isFirstStart)
			{
				var win = new FirstStartWindow();
				win.Owner = instance.window;
				win.ShowDialog();
			}

			if (string.IsNullOrEmpty(Properties.Settings.Default.SongTemplateFile))
			{
				Properties.Settings.Default.SongTemplateFile = Path.Combine(appDir.FullName, "Data", "Standard.ppl");
			}

			// set last portfolio/media directory to "My Documents" and last song directory to song repository

			if (string.IsNullOrEmpty(Properties.Settings.Default.LastPortfolioDirectory))
			{
				Properties.Settings.Default.LastPortfolioDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

			if (string.IsNullOrEmpty(Properties.Settings.Default.LastMediaDirectory))
			{
				Properties.Settings.Default.LastMediaDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

			if (string.IsNullOrEmpty(Properties.Settings.Default.LastSongDirectory))
			{
				Properties.Settings.Default.LastSongDirectory = Properties.Settings.Default.SongsDirectory;
			}

			while (!instance.TryInitDataManager())
			{
				// TODO: this message box is not shown correctly (the first time, when the window is not yet loaded)
				// TODO: show more detailed information about what's wrong (use Exceptions?)
				MessageBox.Show(Resource.seMsgInitDataError);
				instance.window.ShowSettingsWindow();
			}
		}

		private bool TryInitDataManager()
		{
			var fi = new FileInfo(Properties.Settings.Default.SongTemplateFile);
			if (!fi.Exists)
				return false;
			else
				DataManager.SongTemplate = fi;

			return DataManager.TryInitUsingLocal(Properties.Settings.Default.SongsDirectory, Properties.Settings.Default.BackgroundsDirectory);
		}

		#endregion

		#region Public (static) members

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

		public static void AddToPortfolio(Uri uri)
		{
			instance.window.AddToPortfolio(uri);
		}

		public static void ShowSongList()
		{
			bool opened = false;
			foreach (Window win in instance.openedWindows)
			{
				if (win is SongListWindow)
				{
					if (win.WindowState == WindowState.Minimized)
						win.WindowState = WindowState.Normal;

					(win as SongListWindow).FocusSearch();
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
					if (win.WindowState == WindowState.Minimized)
						win.WindowState = WindowState.Normal;

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

		public static void FocusMainWindow(bool tryFocusControlPanel)
		{
			MainWindow win = instance.window;
			if (win.WindowState == WindowState.Minimized)
				win.WindowState = WindowState.Normal;

			win.Activate();
			win.Focus();

			if (tryFocusControlPanel && win.ControlPanel.Child != null)
			{
				win.ControlPanel.Child.Focus();
			}
		}

		public static void DispatchToMainWindow(Action action)
		{
			instance.window.Dispatcher.BeginInvoke(action);
		}

		public static void ReloadActiveMedia()
		{
			instance.window.ReloadActiveMedia();
		}

		public static void TryActivateNext()
		{
			instance.window.TryActivateOffset(1);
		}

		public static void TryActivatePrevious()
		{
			instance.window.TryActivateOffset(-1);
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

		internal static async void CheckForUpdates(bool silent)
		{
			var hc = new HttpClient();
			try
			{
				var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
				var uri = new Uri("http://wordslive.org/version.xml"); 
				var str = await hc.GetStringAsync(uri);
				var reader = new StringReader(str);
				var doc = XDocument.Load(reader);
				var latestVersion = new Version(doc.Root.Element("version").Value);
				var noUpdateVersion = new Version(Properties.Settings.Default.NoUpdateVersion);
				var latestUri = new Uri(doc.Root.Element("url").Value);

				if (currentVersion >= latestVersion)
				{
					if (!silent)
					{
						MessageBox.Show(instance.window, Resource.updMsgUpToDate, "");
					}
				}
				else if (!silent || noUpdateVersion < latestVersion)
				{
					var result = MessageBox.Show(instance.window, String.Format(Resource.updMsgNewVersion, latestVersion.SimplifyVersion().ToString()), "", MessageBoxButton.YesNoCancel);

					if (result == MessageBoxResult.Yes)
					{
						// open download in browser and don't show update alert again
						latestUri.OpenInBrowser();
						Properties.Settings.Default.NoUpdateVersion = latestVersion.ToString();
					}
					else if (result == MessageBoxResult.No)
					{
						// don't show update alert again (until newer version is available)
						Properties.Settings.Default.NoUpdateVersion = latestVersion.ToString();
					}
				}
			}
			catch (Exception)
			{
				if (!silent)
				{
					MessageBox.Show(instance.window, Resource.updMsgFailed, "", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
		#endregion
	}
}
