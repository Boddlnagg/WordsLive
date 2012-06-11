using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.MediaOrderList;
using Words.Core.Songs;
using Words.Core;
using System.Windows;
using Words.Editor;
using Words.Songs;
using System.Configuration;
using System.IO;
using System.Reflection;
using Words.Resources;

namespace Words
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

			LoadAttributes(Assembly.GetAssembly(typeof(Media))); // Words.Core.dll
			LoadAttributes(Assembly.GetExecutingAssembly()); // Words.exe
			LoadAttributes(Assembly.GetAssembly(typeof(Words.Presentation.Wpf.WpfPresentationWindow))); // Words.Presentation.Wpf.dll

			string startupDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
			LoadAttributes(Assembly.LoadFrom(Path.Combine(startupDir, "Words.Slideshow.dll"))); // TODO (Words): automatically load plugins

			InitDataDirectories();
		}

		private void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			UnhandledExceptionWindow win = new UnhandledExceptionWindow(e.Exception);
			win.Owner = window;
			var result = win.ShowDialog();
			if (result.HasValue && result.Value == false)
				e.Handled = true;
		}

		private void LoadAttributes(Assembly assembly)
		{
			Type[] types = assembly.GetTypes();
			MediaManager.RegisterMediaTypes(types);
			IconProviders.RegisterProvidersFromTypes(types);
			ControlPanels.RegisterPanelsFromTypes(types);

			shutdownActions.AddRange(from type in types
									 from method in type.GetMethods()
									 where (method.IsStatic && method.GetParameters().Length == 0 &&
										 method.GetCustomAttributes(typeof(ShutdownAttribute), true).Cast<ShutdownAttribute>().Count() > 0)
									 select (new Action(() => method.Invoke(null, null))));
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

			MediaManager.InitDirectories(Properties.Settings.Default.SongsDirectory, Properties.Settings.Default.BackgroundsDirectory);
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
				if (instance.window.OrderListBox.ActiveItem == null)
					return null;
				else
					return instance.window.OrderListBox.ActiveItem.Data;
			}
		}

		public static void OpenPortfolio(string file)
		{
			instance.window.OpenPortfolio(file);
		}

		public static void AddToPortfolio(string file)
		{
			((MediaOrderList.MediaOrderList)instance.window.OrderListBox.DataContext).Add(MediaManager.LoadMediaMetadata(file));
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

		public static void ShowMainWindow()
		{
			instance.window.Focus();
		}

		public static void ReloadActiveMedia()
		{
			instance.window.ReloadActiveMedia();
		}
		#endregion

		#region Internal members
		internal static void Initialize()
		{
			instance.Init();
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
			//Words.Presentation.Wpf.WpfPresentationWindow.Instance.Close();

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
