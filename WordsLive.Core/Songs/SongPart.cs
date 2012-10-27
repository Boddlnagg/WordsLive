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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MonitoredUndo;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a part of a song.
	/// </summary>
	public class SongPart : INotifyPropertyChanged, ISongElement
	{
		private string name;

		/// <summary>
		/// Gets or sets the name of the part. Must be unique in a song.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value != name)
				{
					if (Root.FindPartByName(value) != null)
					{
						throw new InvalidOperationException("part with that name already exists");
					}

					Undo.ChangeFactory.OnChanging(this, "Name", name, value);
					name = value;
					OnPropertyChanged("Name");
				}
			}
		}
		
		/// <summary>
		/// Gets or sets a list of slides.
		/// </summary>
		public ObservableCollection<SongSlide> Slides { get; private set; }

		/// <summary>
		/// Gets the text of all slides in this part.
		/// Changes to this property are not notified.
		/// </summary>
		public string Text
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.Text).ToArray());
			}
		}

		/// <summary>
		/// Gets the text of all slides in this part, but with chords removed.
		/// Changes to this property are not notified.
		/// </summary>
		public string TextWithoutChords
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.TextWithoutChords).ToArray());
			}
		}

		/// <summary>
		/// Gets a value indicating whether any slide in this part has a translation.
		/// Changes to this property are not notified.
		/// </summary>
		public bool HasTranslation
		{
			get
			{
				return Slides.Where(s => s.HasTranslation).Any();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPart"/> class.
		/// </summary>
		/// <param name="root">The song this part belongs to.</param>
		/// <param name="name">The part's name.</param>
		public SongPart(Song root, string name)
		{
			this.Root = root;

			if (Root.Parts != null && Root.FindPartByName(name) != null)
			{
				throw new InvalidOperationException("part with that name already exists");
			}

			this.name = name;
			this.Slides = new ObservableCollection<SongSlide>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongPart"/> class.
		/// </summary>
		/// <param name="root">The song this part belongs to.</param>
		/// <param name="name">The part's name.</param>
		/// <param name="slides">The slides to add to the part.</param>
		public SongPart(Song root, string name, IEnumerable<SongSlide> slides)
		{
			this.Root = root;

			if (Root.Parts != null && Root.FindPartByName(name) != null)
			{
				throw new InvalidOperationException("part with that name already exists");
			}

			this.name = name;
			this.Slides = new ObservableCollection<SongSlide>(slides);
		}

		/// <summary>
		/// Adds an empty slide to the part (can be undone).
		/// </summary>
		/// <returns>The newly created slide.</returns>
		public SongSlide AddSlide()
		{
			var slide = new SongSlide(Root);
			AddSlide(slide);
			return slide;
		}

		/// <summary>
		/// Adds an existing slide to this part (can be undone).
		/// </summary>
		/// <param name="slide">The slide to add.</param>
		public void AddSlide(SongSlide slide)
		{
			var ch = new DelegateChange(this,
				() => { Slides.Remove(slide); },
				() => { Slides.Add(slide); },
				new ChangeKey<object, string>(this, "Slides"));
			UndoService.Current[Root.UndoKey].AddChange(ch, "AddSlide");
			Slides.Add(slide);
		}

		/// <summary>
		/// Inserts a slide after a specified target slide in this part (can be undone).
		/// </summary>
		/// <param name="slide">The slide to insert.</param>
		/// <param name="target">The target slide. Must be contained in this part.</param>
		public void InsertSlideAfter(SongSlide slide, SongSlide target)
		{
			var index = Slides.IndexOf(target);
			if (index < 0)
				throw new InvalidOperationException("Slide is not in this part.");

			if (index >= Slides.Count - 1)
			{
				AddSlide(slide);
			}
			else
			{
				var ch = new DelegateChange(this,
				() => { Slides.Remove(slide); },
				() => { Slides.Insert(index + 1, slide); },
				new ChangeKey<object, string>(this, "Children"));
				UndoService.Current[Root.UndoKey].AddChange(ch, "InsertSlideAfter");
				this.Slides.Insert(index + 1, slide);
			}
		}

		/// <summary>
		/// Removes a specified slide from this part (can be undone).
		/// </summary>
		/// <param name="slide"></param>
		public void RemoveSlide(SongSlide slide)
		{
			if (Slides.Count <= 1)
				throw new InvalidOperationException("Can't remove last slide in a part.");

			int i = Slides.IndexOf(slide);
			if (i < 0)
				throw new InvalidOperationException("Slide is not in this part.");

			var ch = new DelegateChange(this,
				() => { Slides.Insert(i, slide); },
				() => { Slides.Remove(slide); },
				new ChangeKey<object, string>(this, "Slides"));
			UndoService.Current[Root.UndoKey].AddChange(ch, "RemoveSlide");
			this.Slides.Remove(slide);
		}

		/// <summary>
		/// Duplicates the specified part and inserts the copy
		/// directly after the original slide in the same part (can be undone).
		/// </summary>
		/// <param name="slide">The slide to duplicate.</param>
		/// <returns>The created duplicate.</returns>
		public SongSlide DuplicateSlide(SongSlide slide)
		{
			SongSlide s;
			int i = Slides.IndexOf(slide);
			using (new UndoBatch(Root.UndoKey, "DuplicateSlide", false))
			{
				s = slide.Copy();
				var ch = new DelegateChange(this,
					() => { Slides.Remove(s); },
					() => { Slides.Insert(i + 1, s); },
					new ChangeKey<object, string>(this, "Children"));
				UndoService.Current[Root.UndoKey].AddChange(ch, "DuplicateSlide");
				Slides.Insert(i + 1, s);
			}
			return s;
		}

		/// <summary>
		/// Splits a slide at a specified index. The first part remains in the original slide
		/// and the rest is moved to a new slide (can be undone).
		/// </summary>
		/// <param name="slide">The slide to split.</param>
		/// <param name="splitIndex">The index to split at.</param>
		/// <returns>A new slide containing the rest of the text after the split index.</returns>
		public SongSlide SplitSlide(SongSlide slide, int splitIndex)
		{
			SongSlide newSlide;
			using (new UndoBatch(Root.UndoKey, "SplitSlide", false))
			{
				var textBefore = slide.Text.Substring(0, splitIndex);
				if (textBefore.EndsWith("\r\n"))
					textBefore = textBefore.Substring(0, textBefore.Length - 2);
				var textAfter = slide.Text.Substring(splitIndex);
				if (textAfter.StartsWith("\r\n"))
					textAfter = textAfter.Substring(2);
				newSlide = DuplicateSlide(slide);
				slide.Text = textBefore;
				newSlide.Text = textAfter;
			}
			return newSlide;
		}

		/// <summary>
		/// Sets the background for every slide in the part (can be undone).
		/// </summary>
		/// <param name="bg">The background to use.</param>
		public void SetBackground(SongBackground bg)
		{
			if (!Root.Parts.Contains(this))
			{
				throw new InvalidOperationException("part has not been added to a song");
			}

			using (new UndoBatch(Root.UndoKey, "SetBackground", false))
			{
				int index = Root.AddBackground(bg);

				foreach (var slide in Slides)
				{
					slide.BackgroundIndex = index;
				}

				Root.CleanBackgrounds();
			}
		}

		/// <summary>
		/// Creates a copy of this <see cref="SongPart"/>.
		/// </summary>
		/// <param name="name">The name of the copy.</param>
		/// <returns>The newly created copy.</returns>
		public SongPart Copy(string name)
		{
			return new SongPart(Root, name, Slides.Select(s => s.Copy()));
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public Song Root { get; private set; }

		#endregion
	}
}
