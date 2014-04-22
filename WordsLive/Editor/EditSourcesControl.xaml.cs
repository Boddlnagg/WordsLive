/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using WordsLive.Core.Songs;

namespace WordsLive.Editor
{
	public partial class EditSourcesControl : UserControl
	{
		public static DependencyProperty SongProperty = DependencyProperty.Register(
			"Song", typeof(Song), typeof(EditSourcesControl), new FrameworkPropertyMetadata(SongPropertyChanged));

		public Song Song
		{
			get
			{
				return GetValue(SongProperty) as Song;
			}
			set
			{
				SetValue(SongProperty, value);
			}
		}

		private static void SongPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			
		}

		public EditSourcesControl()
		{
			InitializeComponent();
		}

		private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Delete)
			{
				e.CanExecute = Song.Sources.Count > 1;
				e.Handled = true;
			}
			else if (e.Command == CustomCommands.MoveUp)
			{
				e.CanExecute = Song.Sources.IndexOf((SongSource)e.Parameter) > 0; // in list and not already the first one
				e.Handled = true;
			}
		}

		private void Command_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Delete)
			{
				Song.RemoveSource((SongSource)e.Parameter);
				e.Handled = true;
			}
			else if (e.Command == CustomCommands.MoveUp)
			{
				Song.MoveSourceUp((SongSource)e.Parameter);
			}
			else if (e.Command == ApplicationCommands.New)
			{
				Song.AddSource(String.Empty);
			}
		}
	}
}
