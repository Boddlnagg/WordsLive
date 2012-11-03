using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.MediaOrderList;
using WordsLive.Core.Songs;
using WordsLive.Core;
using System.Windows;
using WordsLive.Editor;
using WordsLive.Songs;
using System.Configuration;
using System.IO;
using System.Reflection;
using WordsLive.Resources;
using Microsoft.Win32;
using System.Threading;
using WordsLive.Core.Data;

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

			InitDataDirectories();

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

		private void InitDataDirectories()
		{
			if (string.IsNullOrEmpty(Properties.Settings.Default.SongsDirectory))
			{
				Properties.Settings.Default.SongsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Powerpraise-Dateien", "Songs");
			}

			if (string.IsNullOrEmpty(Properties.Settings.Default.BackgroundsDirectory))
			{
				Properties.Settings.Default.BackgroundsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Powerpraise-Dateien", "Backgrounds");
			}

			while (!Directory.Exists(Properties.Settings.Default.SongsDirectory) || !Directory.Exists(Properties.Settings.Default.BackgroundsDirectory))
			{
				MessageBox.Show(Resource.sMsgDirectoryMissing);
				window.ShowSettingsWindow();
			}

			DataManager.InitLocalDirectories(Properties.Settings.Default.SongsDirectory, Properties.Settings.Default.BackgroundsDirectory);
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

		/// <summary>
		/// Creates a new instance of <see cref="Song"/> from the template file (the SongTemplateFile application setting).
		/// This is used for creating new (empty) songs and for importing songs.
		/// TODO: move to Core assembly
		/// </summary>
		/// <returns>The created song.</returns>
		public static Song CreateSongFromTemplate()
		{
			// init song template file
			var template = Properties.Settings.Default.SongTemplateFile;

			if (string.IsNullOrEmpty(template) || !File.Exists(template))
			{
				// fall back to standard template in data directory
				template = Path.Combine("Data", "Standard.ppl");
			}

			return new Song(template, DataManager.LocalFiles) { SongTitle = WordsLive.Resources.Resource.eNewSongTitle };
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
