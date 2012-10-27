/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using System.Linq;
using MonitoredUndo;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a slide in a song.
	/// </summary>
	public class SongSlide : INotifyPropertyChanged, ISongElement
	{
		private string text;
		private string translation;
		private int size;
		private bool hasTranslation;
		private bool hasChords;

		/// <summary>
		/// Gets or sets the text on this slide.
		/// </summary>
		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (value != text)
				{
					if (value == null)
						value = String.Empty;

					Undo.ChangeFactory.OnChangingTryMerge(this, "Text", text, value);
					text = value;
					HasChords = Chords.Chords.GetChords(text).Any();
					TextWithoutChords = Chords.Chords.RemoveAll(text);
					OnNotifyPropertyChanged("Text");
					OnNotifyPropertyChanged("TextWithoutChords");
				}
			}
		}

		/// <summary>
		/// Gets the text on this slide, but with chords removed.
		/// </summary>
		public string TextWithoutChords { get; private set; }

		/// <summary>
		/// Gets or sets the translation of the text on this slide.
		/// </summary>
		public string Translation
		{
			get
			{
				return translation;
			}
			set
			{
				if (value != translation)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "Translation", translation, value);
					translation = value;
					HasTranslation = !String.IsNullOrEmpty(translation);
					OnNotifyPropertyChanged("Translation");
				}
			}
		}

		/// <summary>
		/// Gets or sets the index pointing to the background of this slide.
		/// </summary>
		public int BackgroundIndex { get; set; }

		/// <summary>
		/// Gets or sets the font size of the text on this slide.
		/// Whenever this is changed, the song's main font size is also changed.
		/// </summary>
		public int Size
		{
			get
			{
				return size;
			}
			set
			{
				int oldMainSize = Root.Formatting.MainText.Size;
				int oldSize = this.size;

				Action undo = () => {
					this.size = oldSize;
					Root.Formatting.MainText.Size = oldMainSize;
				};
				Action redo = () => { 
					this.size = value;
					Root.Formatting.MainText.Size = this.size;
				};

				var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "TextSize"));
				UndoService.Current[Root.UndoKey].AddChange(ch, "ChangeTextSize");
				redo();

				OnNotifyPropertyChanged("Size");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this slide has a translation
		/// </summary>
		public bool HasTranslation
		{
			get
			{
				return hasTranslation;
			}
			private set
			{
				if (value != hasTranslation)
				{
					hasTranslation = value;
					OnNotifyPropertyChanged("HasTranslation");
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this slide has chords.
		/// </summary>
		public bool HasChords
		{
			get
			{
				return hasChords;
			}
			private set
			{
				if (value != hasChords)
				{
					hasChords = value;
					OnNotifyPropertyChanged("HasChords");
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongSlide"/> class.
		/// </summary>
		/// <param name="root">The song this slide belongs to.</param>
		public SongSlide(Song root)
		{
			this.Root = root;
		}

		/// <summary>
		/// Clones this slide.
		/// </summary>
		/// <returns>A clone of this slide.</returns>
		public SongSlide Clone()
		{
			var s = new SongSlide(Root);
			s.Text = Text;
			s.Translation = Translation;
			s.BackgroundIndex = BackgroundIndex;
			s.Size = Size;
			return s;
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public Song Root { get; private set; }

		#endregion
	}
}
