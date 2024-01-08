/*
 * WordsLive - worship projection software
 * Copyright (c) 2015 Patrick Reisert
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
using System.Windows.Input;
using WordsLive.Utils;

namespace WordsLive
{
	// TODO: move some commands to class where they are used (use DelegateCommand?)
	public static class CustomCommands
	{
		#region Valid Custom RoutedCommands
		public static LocalizedRoutedCommand Exit
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand SwitchWindow
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand Rename
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand Insert
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand MoveUp
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand MoveDown
		{
			get;
			private set;
		}

		#endregion

		// TODO: this should be a (global?) delegate command
		public static LocalizedRoutedCommand SearchSongSelect
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand OpenInEditor
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ShowSettings
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ChooseBackground
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ChoosePresentationArea
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand AddMedia
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand HidePresentation
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand Blackscreen
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ShowPresentation
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ToggleBlackscreen
		{
			get;
			private set;
		}

		#region Only used in Viewer
		// These should probably not be routed commands
		public static LocalizedRoutedCommand CheckForUpdates
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ShowAboutDialog
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand Activate
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand EditActive
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ShowTestImage
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ShowNotification
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand RotateLeft
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand RotateRight
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand CreateSlideshow
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand SubmitCcliOlr
		{
			get;
			private set;
		}
		#endregion

		#region Only used in Editor
		// These should probably not be routed commands
		public static LocalizedRoutedCommand Export
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ImportFromClipboard
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ViewCurrent
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand AddPart
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand Duplicate
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand Split
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand SwapTextAndTranslation
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand ChangeTranslationDisplayOptions
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand SongSettings
		{
			get;
			private set;
		}

		public static LocalizedRoutedCommand EditChords
		{
			get;
			private set;
		}
		#endregion

		static CustomCommands()
		{
			Type t = typeof(CustomCommands);

			Exit = new LocalizedRoutedCommand("Exit", t, new InputGestureCollection { new KeyGesture(Key.F4, ModifierKeys.Alt) });
			CheckForUpdates = new LocalizedRoutedCommand("CheckForUpdates", t);
			ShowAboutDialog = new LocalizedRoutedCommand("ShowAboutDialog", t);
			SwitchWindow = new LocalizedRoutedCommand("SwitchWindow", t, new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) });
			EditActive = new LocalizedRoutedCommand("EditActive", t);
			OpenInEditor = new LocalizedRoutedCommand("OpenInEditor", t);
			Export = new LocalizedRoutedCommand("Export", t);
			ImportFromClipboard = new LocalizedRoutedCommand("ImportFromClipboard", t, new InputGestureCollection { new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift) });
			Activate = new LocalizedRoutedCommand("Activate", t);
			ShowSettings = new LocalizedRoutedCommand("ShowSettings", t);
			ViewCurrent = new LocalizedRoutedCommand("ViewCurrent", t);
			ChooseBackground = new LocalizedRoutedCommand("ChooseBackground", t);
			ChoosePresentationArea = new LocalizedRoutedCommand("ChoosePresentationArea", t);
			Rename = new LocalizedRoutedCommand("Rename", t, new InputGestureCollection { new KeyGesture(Key.F2) });
			Insert = new LocalizedRoutedCommand("Insert", t, new InputGestureCollection { new KeyGesture(Key.Insert) });
			MoveUp = new LocalizedRoutedCommand("MoveUp", t, new InputGestureCollection { new KeyGesture(Key.Up, ModifierKeys.Control) });
			MoveDown = new LocalizedRoutedCommand("MoveDown", t, new InputGestureCollection { new KeyGesture(Key.Down, ModifierKeys.Control) });
			AddPart = new LocalizedRoutedCommand("AddPart", t);
			Duplicate = new LocalizedRoutedCommand("Duplicate", t);
			Split = new LocalizedRoutedCommand("Split", t);
			SwapTextAndTranslation = new LocalizedRoutedCommand("SwapTextAndTranslation", t);
			ChangeTranslationDisplayOptions = new LocalizedRoutedCommand("ChangeTranslationDisplayOptions", t);
			SongSettings = new LocalizedRoutedCommand("SongSettings", t);
			EditChords = new LocalizedRoutedCommand("EditChords", t);
			AddMedia = new LocalizedRoutedCommand("AddMedia", t, new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Control) });
			HidePresentation = new LocalizedRoutedCommand("HidePresentation", t, new InputGestureCollection { new KeyGesture(Key.F6) });
			Blackscreen = new LocalizedRoutedCommand("Blackscreen", t, new InputGestureCollection { new KeyGesture(Key.F7) });
			ShowPresentation = new LocalizedRoutedCommand("ShowPresentation", t, new InputGestureCollection { new KeyGesture(Key.F8) });
			ToggleBlackscreen = new LocalizedRoutedCommand("ToggleBlackscreen", t); // TODO: add configurable keyboard shortcut
			ShowTestImage = new LocalizedRoutedCommand("ShowTestImage", t);
			ShowNotification = new LocalizedRoutedCommand("ShowNotification", t);
			RotateLeft = new LocalizedRoutedCommand("RotateLeft", t);
			RotateRight = new LocalizedRoutedCommand("RotateRight", t);
			CreateSlideshow = new LocalizedRoutedCommand("CreateSlideshow", t);
			SubmitCcliOlr = new LocalizedRoutedCommand("SubmitCcliOlr", t);
			SearchSongSelect = new LocalizedRoutedCommand("SearchSongSelect", t);
		}
	}
}
