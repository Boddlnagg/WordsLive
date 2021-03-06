﻿/*
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
using System.ComponentModel;
using System.Windows;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.Chords;

namespace WordsLive.Editor
{
	public partial class EditChordsWindow : Window, INotifyPropertyChanged, IDataErrorInfo
	{
		private Key originalKey = null;
		private int transposeAmount;
		private bool germanNotation;
		private bool longChordNames;

		private Song song;

		public EditChordsWindow(Song song)
		{
			InitializeComponent();

			this.song = song;

			DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		public string OriginalKey
		{
			get
			{
				return originalKey == null ? String.Empty : originalKey.ToString();
			}
			set
			{
				try
				{
					originalKey = new Key(value);
				}
				catch (ArgumentException)
				{
					originalKey = null;
				}
				OnNotifyPropertyChanged("OriginalKey");
				OnNotifyPropertyChanged("TargetKey");
			}
		}

		public int TransposeAmount
		{
			get
			{
				return transposeAmount;
			}
			set
			{
				transposeAmount = Note.NormalizeSemitones(value);
				OnNotifyPropertyChanged("TransposeAmount");
				OnNotifyPropertyChanged("TargetKey");
			}
		}

		public string TargetKey
		{
			get
			{
				return originalKey == null ? String.Empty : originalKey.Transpose(TransposeAmount).ToString();
			}
		}

		public bool GermanNotation
		{
			get
			{
				return germanNotation;
			}
			set
			{
				if (!value)
					LongChordNames = false;

				germanNotation = value;
				Chords.GermanNotation = germanNotation;
				OnNotifyPropertyChanged("GermanNotation");
			}
		}

		public bool LongChordNames
		{
			get
			{
				return longChordNames;
			}
			set
			{
				longChordNames = value;
				Chords.LongChordNames = longChordNames;
				OnNotifyPropertyChanged("LongChordNames");
			}
		}

		private void ButtonTranspose_Click(object sender, RoutedEventArgs e)
		{
			Chords.Transpose(song, originalKey, TransposeAmount);
			OriginalKey = TargetKey;
			TransposeAmount = TransposeAmount;
		}

		public string Error
		{
			get { return null; }
		}

		public string this[string name]
		{
			get
			{
				switch (name)
				{
					case "OriginalKey":
						if (this.originalKey == null)
							return "Geben Sie eine gültige Tonart ein.";
						break;
				}

				return null;
			}
		}

		private void ButtonRemoveChords_Click(object sender, RoutedEventArgs e)
		{
			Chords.RemoveAll(song);
		}
	}
}
