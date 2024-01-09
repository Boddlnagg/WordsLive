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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WordsLive.Core;
using WordsLive.Core.Songs;

namespace WordsLive.Songs
{
	[TargetMedia(typeof(SongMedia))]
	public partial class SongControlPanel : UserControl, IMediaControlPanel
	{
		private class SongSlideSelection
		{
			public string PartName { get; set; }
			public int PartIndex { get; set; }
			public int SlideIndex { get; set; }
		}

		private SongMedia song;
		private bool translationDisplayOptionsApplied;
		private bool finishedLoading;
		private SongPresentation presentation;
		private SongPresentation oldPresentation;

		public SongControlPanel()
		{
			this.InitializeComponent();
		}

		SongSlideSelection recoverSelection;

		public void Init(Media media)
		{
			SongMedia s = (media as SongMedia);

			if (s == null)
				throw new ArgumentException("media must be not null and a Song");

			bool updating = this.song != null; // whether we are updating

			this.song = s;
			this.translationDisplayOptionsApplied = false;

			if (s.Song.HasTranslation)
			{
				switch (s.TranslationDisplayOptions)
				{
					case TranslationDisplayOptions.Hide:
						DoRemoveTranslation();
						translationDisplayOptionsApplied = true;
						break;
					case TranslationDisplayOptions.Only:
						DoSwapTextAndTranslation();
						DoRemoveTranslation();
						translationDisplayOptionsApplied = true;
						break;
					case TranslationDisplayOptions.Swap:
						DoSwapTextAndTranslation();
						translationDisplayOptionsApplied = true;
						break;
					default:
						// nothing to do
						break;
				}
			}

			Refresh(!updating);
		}

		void Refresh(bool firstTime)
		{
			if (!firstTime)
			{
				oldPresentation = presentation;
				oldPresentation.FinishedLoading -= pres_OnFinishedLoading;
			}

			finishedLoading = false;
			presentation = Controller.PresentationManager.CreatePresentation<SongPresentation>();
			presentation.FinishedLoading += pres_OnFinishedLoading;
			if (!firstTime)
			{
				presentation.ShowChords = oldPresentation.ShowChords;
			}
			presentation.Load(this.song.Song, !firstTime);

			if (!firstTime)
			{
				if (this.SlideListBox.SelectedItem != null)
				{
					var con = (SongSlideListBox.SongSlideContainer)this.SlideListBox.SelectedItem;
					recoverSelection = new SongSlideSelection { PartName = con.PartName, SlideIndex = con.SlideIndex, PartIndex = con.PartIndex };
				}
			}

			this.SlideListBox.Song = this.song.Song;

			if (!firstTime && recoverSelection != null)
			{
				this.SlideListBox.SelectedIndex = RecoverSelectedSlide(recoverSelection);
			}
			else
			{
				this.SlideListBox.Loaded += (sender, args) => this.SlideListBox.SelectedIndex = -1;
			}
		}

		void pres_OnFinishedLoading(object sender, EventArgs args)
		{
			finishedLoading = true;
			recoverSelection = null;
			if (this.SlideListBox.SelectedIndex >= 0)
			{
				presentation.CurrentSlideIndex = this.SlideListBox.SelectedIndex;

				if (Controller.PresentationManager.CurrentPresentation != oldPresentation && oldPresentation != null)
					oldPresentation.Close();
				Controller.PresentationManager.CurrentPresentation = presentation;

				oldPresentation = null;
				
			}
		}

		private int RecoverSelectedSlide(SongSlideSelection selection)
		{
			int i = 1;
			int dist = -1;
			int slideCount = 1;
			int index = -1;
			SongPart part = null;

			foreach (var partRef in song.Song.Order)
			{
				if (partRef.Part.Name == selection.PartName)
				{
					var newDist = Math.Abs(selection.PartIndex - i);
					if (i > selection.PartIndex && newDist > dist && dist >= 0)
						break;
					dist = newDist;
					part = partRef.Part;
					index = slideCount;
				}
				slideCount += partRef.Part.Slides.Count;
				i++;
			}

			if (part != null)
				return index + (selection.SlideIndex >= part.Slides.Count ? part.Slides.Count - 1 : selection.SlideIndex);
			else
				return 0;
		}

		public Control Control
		{
			get
			{
				return this;
			}
		}


		public Media Media
		{
			get
			{
				return song;
			}
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (finishedLoading && this.SlideListBox.SelectedIndex >= 0)
			{
				if (Controller.PresentationManager.CurrentPresentation != presentation)
					Controller.PresentationManager.CurrentPresentation = presentation;
				presentation.CurrentSlideIndex = this.SlideListBox.SelectedIndex;
			}
		}

		private void ListBox_SelectNext(object sender, RoutedEventArgs e)
		{
			Controller.TryActivateNext();
		}

		private void ListBox_SelectPrevious(object sender, RoutedEventArgs e)
		{
			Controller.TryActivatePrevious();
		}

		public bool IsUpdatable
		{
			get { return true; }
		}

		public ControlPanelLoadState LoadState
		{
			get { return ControlPanelLoadState.Loaded; }
		}

		public bool ShowChords
		{
			get
			{
				return presentation.ShowChords;
			}
			set
			{
				presentation.ShowChords = value;
			}
		}

		private void DoSwapTextAndTranslation()
		{
			foreach (var part in song.Song.Parts)
			{
				part.SwapTextAndTranslation();
			}
		}

		private void DoRemoveTranslation()
		{
			foreach (var part in song.Song.Parts)
			{
				part.RemoveTranslation();
			}
		}

		private void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.ChangeTranslationDisplayOptions)
			{
				e.CanExecute = translationDisplayOptionsApplied || song.Song.HasTranslation;
				e.Handled = true;
			}
		}

		private void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == CustomCommands.ChangeTranslationDisplayOptions)
			{
				var win = new TranslationDisplayOptionsWindow(song);
				win.Owner = Application.Current.MainWindow;
				win.ShowDialog();

				// apply new translation settings
				MediaManager.LoadMedia(song); // ugly hack to obtain the original text and translation (by reloading from disk)
				Init(song);

				// make sure enabled state of the menu item is correct
				Keyboard.Focus(this.SlideListBox);
				
				e.Handled = true;
			}
		}

		public void Close()
		{
			if (Controller.PresentationManager.CurrentPresentation != presentation)
				presentation.Close();
		}
	}
}