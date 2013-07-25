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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

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
		[JsonIgnore]
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
		[JsonIgnore]
		public string TextWithoutChords
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.TextWithoutChords).ToArray());
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
			SongSlide slide;

			slide = new SongSlide(Root);
			AddSlide(slide);

			return slide;
		}

		/// <summary>
		/// Adds an existing slide to this part (can be undone).
		/// </summary>
		/// <param name="slide">The slide to add.</param>
		public void AddSlide(SongSlide slide)
		{
			Undo.ChangeFactory.OnChanging(this,
				() => { Slides.Remove(slide); },
				() => { Slides.Add(slide); },
				"AddSlide");
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
				Undo.ChangeFactory.OnChanging(this,
					() => { Slides.Remove(slide); },
					() => { Slides.Insert(index + 1, slide); },
					"InsertSlideAfter");
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

			Undo.ChangeFactory.OnChanging(this,
				() => { Slides.Insert(i, slide); },
				() => { Slides.Remove(slide); },
				"RemoveSlide");
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
			using (Undo.ChangeFactory.Batch(this, "DuplicateSlide"))
			{
				s = slide.Copy();
				Undo.ChangeFactory.OnChanging(this,
					() => { Slides.Remove(s); },
					() => { Slides.Insert(i + 1, s); },
					"DuplicateSlide");
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
			using (Undo.ChangeFactory.Batch(this, "SplitSlide"))
			{
				var textBefore = slide.Text.Substring(0, splitIndex);
				if (textBefore.EndsWith("\r\n"))
					textBefore = textBefore.Substring(0, textBefore.Length - 2);
				else if (textBefore.EndsWith("\n"))
					textBefore = textBefore.Substring(0, textBefore.Length - 1);
				var textAfter = slide.Text.Substring(splitIndex);
				if (textAfter.StartsWith("\r\n"))
					textAfter = textAfter.Substring(2);
				else if (textAfter.StartsWith("\n"))
					textAfter = textAfter.Substring(1);
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

			if (bg.Type == SongBackgroundType.Video)
			{
				throw new InvalidOperationException("Can't set video background to part only");
			}

			if (Root.VideoBackground != null)
			{
				throw new InvalidOperationException("Can't set background of part only if a video background is used");
			}

			using (Undo.ChangeFactory.Batch(this, "SetBackground"))
			{
				int index = Root.AddBackground(bg);

				foreach (var slide in Slides)
				{
					slide.BackgroundIndex = index;
				}

				Root.CleanBackgrounds();
			}
		}

		public void SwapTextAndTranslation()
		{
			using (Undo.ChangeFactory.Batch(this, "SwapTextAndTranslation"))
			{
				foreach (var s in Slides)
				{
					s.SwapTextAndTranslation();
				}
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

		[JsonIgnore]
		public Song Root { get; private set; }

		#endregion
	}
}
