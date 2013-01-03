using System;
using System.Windows.Input;

namespace WordsLive
{
	// TODO: move some commands to class where they are used
	public static class CustomCommands
	{
		public static RoutedCommand Exit
		{
			get;
			private set;
		}

		public static RoutedCommand ShowSonglist
		{
			get;
			private set;
		}

		public static RoutedCommand SwitchWindow
		{
			get;
			private set;
		}

		public static RoutedCommand EditActive
		{
			get;
			private set;
		}

		// TODO: move to editor class
		public static RoutedCommand OpenInEditor
		{
			get;
			private set;
		}

		public static RoutedCommand Activate
		{
			get;
			private set;
		}

		public static RoutedCommand ShowSettings
		{
			get;
			private set;
		}

		public static RoutedCommand ViewCurrent
		{
			get;
			private set;
		}

		public static RoutedCommand ChooseBackground
		{
			get;
			private set;
		}

		public static RoutedCommand ChoosePresentationArea
		{
			get;
			private set;
		}

		public static RoutedCommand Rename
		{
			get;
			private set;
		}

		public static RoutedCommand Insert
		{
			get;
			private set;
		}

		public static RoutedCommand MoveUp
		{
			get;
			private set;
		}

		public static RoutedCommand MoveDown
		{
			get;
			private set;
		}

		public static RoutedCommand AddPart
		{
			get;
			private set;
		}

		public static RoutedCommand Duplicate
		{
			get;
			private set;
		}

		public static RoutedCommand Split
		{
			get;
			private set;
		}

		public static RoutedCommand SwapTextAndTranslation
		{
			get;
			private set;
		}

		public static RoutedCommand SongSettings
		{
			get;
			private set;
		}

		public static RoutedCommand EditChords
		{
			get;
			private set;
		}

		public static RoutedCommand AddMedia
		{
			get;
			private set;
		}

		public static RoutedCommand HidePresentation
		{
			get;
			private set;
		}

		public static RoutedCommand Blackscreen
		{
			get;
			private set;
		}

		public static RoutedCommand ShowPresentation
		{
			get;
			private set;
		}

		public static RoutedCommand ShowTestImage
		{
			get;
			private set;
		}

		public static RoutedCommand RotateLeft
		{
			get;
			private set;
		}

		public static RoutedCommand RotateRight
		{
			get;
			private set;
		}

		static CustomCommands()
		{
			Type t = typeof(CustomCommands);

			Exit = new RoutedCommand("Exit", t);
			ShowSonglist = new RoutedCommand("ShowSonglist", t, new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control) });
			SwitchWindow = new RoutedCommand("SwitchWindow", t, new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) });
			EditActive = new RoutedCommand("EditActive", t);
			OpenInEditor = new RoutedCommand("OpenInEditor", t);
			Activate = new RoutedCommand("Activate", t);
			ShowSettings = new RoutedCommand("ShowSettings", t);
			ViewCurrent = new RoutedCommand("ViewCurrent", t);
			ChooseBackground = new RoutedCommand("ChooseBackground", t);
			ChoosePresentationArea = new RoutedCommand("ChoosePresentationArea", t);
			Rename = new RoutedCommand("Rename", t, new InputGestureCollection { new KeyGesture(Key.F2) });
			Insert = new RoutedCommand("Insert", t, new InputGestureCollection { new KeyGesture(Key.Insert) });
			MoveUp = new RoutedCommand("MoveUp", t);
			MoveDown = new RoutedCommand("MoveDown", t);
			AddPart = new RoutedCommand("AddPart", t);
			Duplicate = new RoutedCommand("Duplicate", t);
			Split = new RoutedCommand("Split", t);
			SwapTextAndTranslation = new RoutedCommand("SwapTextAndTranslation", t);
			SongSettings = new RoutedCommand("SongSettings", t);
			EditChords = new RoutedCommand("EditChords", t);
			AddMedia = new RoutedCommand("AddMedia", t);
			HidePresentation = new RoutedCommand("HidePresentation", t, new InputGestureCollection { new KeyGesture(Key.F6) });
			Blackscreen = new RoutedCommand("Blackscreen", t, new InputGestureCollection { new KeyGesture(Key.F7) });
			ShowPresentation = new RoutedCommand("ShowPresentation", t, new InputGestureCollection { new KeyGesture(Key.F8) });
			ShowTestImage = new RoutedCommand("ShowTestImage", t);
			RotateLeft = new RoutedCommand("RotateLeft", t);
			RotateRight = new RoutedCommand("RotateRight", t);
		}
	}
}
