using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Words
{
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

		public static RoutedCommand ShowEditor
		{
			get;
			private set;
		}

		public static RoutedCommand ShowViewer
		{
			get;
			private set;
		}

		public static RoutedCommand EditActive
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

		static CustomCommands()
		{
			Type t = typeof(CustomCommands);

			Exit = new RoutedCommand("Exit", t);
			ShowSonglist = new RoutedCommand("ShowSonglist", t);
			ShowEditor = new RoutedCommand("ShowEditor", t);
			ShowViewer = new RoutedCommand("ShowViewer", t);
			EditActive = new RoutedCommand("EditActive", t);
			ShowSettings = new RoutedCommand("ShowSettings", t);
			ViewCurrent = new RoutedCommand("ViewCurrent", t);
			ChooseBackground = new RoutedCommand("ChooseBackground", t);
			ChoosePresentationArea = new RoutedCommand("ChoosePresentationArea", t);
			Rename = new RoutedCommand("Rename", t, new InputGestureCollection { new KeyGesture(Key.F2) });
			Insert = new RoutedCommand("Insert", t, new InputGestureCollection { new KeyGesture(Key.Insert) });
			AddPart = new RoutedCommand("AddPart", t);
			Duplicate = new RoutedCommand("Duplicate", t);
			Split = new RoutedCommand("Split", t);
			SwapTextAndTranslation = new RoutedCommand("SwapTextAndTranslation", t);
			SongSettings = new RoutedCommand("SongSettings", t);
			EditChords = new RoutedCommand("EditChords", t);
		}
	}
}
