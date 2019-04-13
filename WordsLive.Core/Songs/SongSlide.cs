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
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a slide in a song.
	/// </summary>
	public class SongSlide : INotifyPropertyChanged, ISongElementWithSize
	{
		private string text;
		private string translation;
		private int size;
		private bool hasTranslation;
		private bool hasChords;
		private int backgroundIndex;

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
					if (HasChords)
						TextWithoutChords = Chords.Chords.RemoveAll(text);
					else
						TextWithoutChords = Text;
					OnPropertyChanged("Text");
					OnPropertyChanged("TextWithoutChords");
				}
			}
		}

		/// <summary>
		/// Gets the text on this slide, but with chords removed.
		/// </summary>
		[JsonIgnore]
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
					bool translationHasChords = HasTranslation && Chords.Chords.GetChords(translation).Any();
					if (translationHasChords)
						TranslationWithoutChords = Chords.Chords.RemoveAll(translation);
					else
						TranslationWithoutChords = translation;
					OnPropertyChanged("Translation");
					OnPropertyChanged("TranslationWithoutChords");
				}
			}
		}

		/// <summary>
		/// Gets the translation of the text on this slide, but with chords removed.
		/// </summary>
		[JsonIgnore]
		public string TranslationWithoutChords { get; private set; }

		/// <summary>
		/// Gets or sets the index pointing to the background of this slide.
		/// </summary>
		public int BackgroundIndex
		{
			get
			{
				return backgroundIndex;
			}
			set
			{
				// need to notify even if the index stays the same!
				Undo.ChangeFactory.OnChanging(this, "BackgroundIndex", backgroundIndex, value);
				backgroundIndex = value;
				OnPropertyChanged("BackgroundIndex");
				OnPropertyChanged("Background");
			}
		}

		/// <summary>
		/// Gets the background that is assigned to this slide.
		/// </summary>
		[JsonIgnore]
		public SongBackground Background
		{
			get
			{
				return Root.Backgrounds[BackgroundIndex];
			}
		}

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
				using (Undo.ChangeFactory.Batch(this, "ChangeTextSize"))
				{
					Undo.ChangeFactory.OnChanging(this, "Size", size, value);
					size = value;
					if (Root.Formatting.MainText.Size != size)
					{
						var formatting = Root.Formatting;
						formatting.MainText.Size = value;
						Root.Formatting = formatting;
					}
				}

				OnPropertyChanged("Size");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this slide has a translation
		/// </summary>
		[JsonIgnore]
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
					OnPropertyChanged("HasTranslation");
					Root.UpdateHasTranslation(value);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this slide has chords.
		/// </summary>
		[JsonIgnore]
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
					OnPropertyChanged("HasChords");
					Root.UpdateHasChords(value);
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
			this.text = "";
			this.TextWithoutChords = "";
			this.translation = "";
			this.TranslationWithoutChords = "";
			this.size = Root.Formatting.MainText.Size;
		}

		/// <summary>
		/// Sets the background that is assigned to this slide.
		/// </summary>
		/// <param name="bg">The background to use.</param>
		public void SetBackground(SongBackground bg)
		{
			if (bg.Type == SongBackgroundType.Video)
			{
				throw new InvalidOperationException("Can't set video background to slide only");
			}

			if (Root.VideoBackground != null)
			{
				throw new InvalidOperationException("Can't set background of slide only if a video background is used");
			}

			using (Undo.ChangeFactory.Batch(this, "SetBackground"))
			{
				int index = Root.AddBackground(bg);
				this.BackgroundIndex = index;
				Root.CleanBackgrounds();
			}
		}

		public void SwapTextAndTranslation()
		{
			using (Undo.ChangeFactory.Batch(this, "SwapTextAndTranslation"))
			{
				var tmp = this.Translation;
				this.Translation = this.Text;
				this.Text = tmp;
			}
		}

		/// <summary>
		/// Copies this slide.
		/// </summary>
		/// <returns>A copy of this slide.</returns>
		public SongSlide Copy()
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

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		[JsonIgnore]
		public Song Root { get; private set; }

		#endregion
	}
}
