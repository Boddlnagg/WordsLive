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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WordsLive.Core.Songs.Undo;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a song media object.
	/// </summary>
	public class Song : Media, INotifyPropertyChanged, ISongElement
	{
		private string songTitle;
		private string category;
		private string language;
		private string translationLanguage;
		private string comment;
		private string copyright;
		private SongFormatting formatting;

		#region Undo/Redo

		private UndoManager undoManager;
		private bool isUndoEnabled = false;

		internal UndoKey UndoKey { get; private set; }

		public UndoManager UndoManager
		{
			get
			{
				if (!isUndoEnabled)
					throw new InvalidOperationException("Undo is currently disabled.");

				if (undoManager == null)
					undoManager = new UndoManager(MonitoredUndo.UndoService.Current[UndoKey]);
				
				return undoManager;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether undo is enabled for this song.
		/// This is <c>false</c> by default.
		/// When it has been <c>true</c> and is set to <c>false</c>, the undo/redo stack
		/// is cleared and cannot be restored again.
		/// </summary>
		public bool IsUndoEnabled
		{
			get
			{
				return isUndoEnabled;
			}
			set
			{
				if (!value && isUndoEnabled)
				{
					// disable undo -> clear stack
					UndoManager.Root.Clear();
				}

				isUndoEnabled = value;
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the song title.
		/// </summary>
		public string SongTitle
		{
			get
			{
				return songTitle;
			}
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value");

				if (value != songTitle)
				{
					Undo.ChangeFactory.OnChanging(this, "SongTitle", songTitle, value);
					songTitle = value;
					OnPropertyChanged("SongTitle");
					OnPropertyChanged("Title");
				}
			}
		}

		/// <summary>
		/// Gets or sets the category of this song.
		/// </summary>
		public string Category
		{
			get
			{
				return category;
			}
			set
			{
				if (value != category)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "Category", category, value);
					category = value;
					OnPropertyChanged("Category");
				}
			}
		}

		/// <summary>
		/// Gets or sets the language of this song.
		/// </summary>
		public string Language
		{
			get
			{
				return language;
			}
			set
			{
				if (value != language)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "Language", language, value);
					language = value;
					OnPropertyChanged("Language");
				}
			}
		}

		/// <summary>
		/// Gets or sets the language of the translation.
		/// TODO: this setting is currently not saved (not supported in .ppl)
		/// </summary>
		public string TranslationLanguage
		{
			get
			{
				return translationLanguage;
			}
			set
			{
				if (value != translationLanguage)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "TranslationLanguage", translationLanguage, value);
					translationLanguage = value;
					OnPropertyChanged("TranslationLanguage");
				}
			}
		}

		/// <summary>
		/// Gets or sets the comment for this song.
		/// </summary>
		public string Comment
		{
			get
			{
				return comment;
			}
			set
			{
				if (value != comment)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "Comment", comment, value);
					comment = value;
					OnPropertyChanged("Comment");
				}
			}
		}

		/// <summary>
		/// Gets or sets the copyright information of this song.
		/// </summary>
		public string Copyright
		{
			get
			{
				return copyright;
			}
			set
			{
				if (value != copyright)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "Copyright", copyright, value);
					copyright = value;
					OnPropertyChanged("Copyright");
				}
			}
		}

		/// <summary>
		/// Gets a list of sources.
		/// </summary>
		public ObservableCollection<SongSource> Sources { get; private set; }

		/// <summary>
		/// Gets or sets a list of backgrounds.
		/// </summary>
		public ObservableCollection<SongBackground> Backgrounds { get; private set; }

		/// <summary>
		/// Gets the video background used for this song or <c>null</c> if no video background is used.
		/// </summary>
		public SongBackground VideoBackground
		{
			get
			{
				return Backgrounds[0].Type == SongBackgroundType.Video ? Backgrounds[0] : null;
			}
		}

		/// <summary>
		/// Gets or sets the formatting for this song.
		/// </summary>
		public SongFormatting Formatting
		{
			get
			{
				return formatting;
			}
			set
			{
				Undo.ChangeFactory.OnChanging(this, "Formatting", formatting == null ? null : formatting.Clone(), value);
				formatting = value;
				OnPropertyChanged("Formatting");
			}
		}
		
		/// <summary>
		/// Gets a list of song parts.
		/// </summary>
		public ObservableCollection<SongPart> Parts { get; private set; }
		
		/// <summary>
		/// Gets or sets the order of song parts indicated by a list of song part references.
		/// </summary>
		public ObservableCollection<SongPartReference> Order { get; private set; }

		/// <summary>
		/// Gets the text of all parts at once.
		/// Changes to this property are not notified.
		/// </summary>
		public string Text
		{
			get
			{
				return string.Join("\n", Parts.Select(part => part.Text).ToArray());
			}
		}

		/// <summary>
		/// Gets the text of all parts but with chords symbols removed.
		/// Changes to this property are not notified.
		/// </summary>
		public string TextWithoutChords
		{
			get
			{
				return string.Join("\n", Parts.Select(part => part.TextWithoutChords).ToArray());
			}
		}

		/// <summary>
		/// Gets the first slide of this song or <c>null</c> if the song has no slides.
		/// Changes to this property are not notified.
		/// </summary>
		public SongSlide FirstSlide
		{
			get
			{
				if (this.Order.Count == 0)
					return null;
				return (from p in this.Parts where p.Name == this.Order[0].Part.Name select p.Slides[0]).Single();
			}
		}

		/// <summary>
		/// Gets the last slide of this song or <c>null</c> if the song has no slides.
		/// Changes to this property are not notified.
		/// </summary>
		public SongSlide LastSlide
		{
			get
			{
				if (this.Order.Count == 0)
					return null;
				return (from p in this.Parts where p.Name == this.Order[this.Order.Count - 1].Part.Name select p.Slides[p.Slides.Count - 1]).Single();
			}
		}

		/// <summary>
		/// Gets a value indicating whether any slide in this song has a translation.
		/// Changes to this property are not notified.
		/// </summary>
		public bool HasTranslation
		{
			get
			{
				return this.Parts.Any((part) => part.HasTranslation);
			}
		}

		/// <summary>
		/// Gets a valule indicating whether any slide in this song has chords.
		/// Changes to this property are not notified.
		/// </summary>
		public bool HasChords
		{
			get
			{
				return Chords.Chords.GetChords(Text).Any();
			}
		}

		/// <summary>
		/// Finds a part using a name.
		/// </summary>
		/// <param name="reference">The name of the part to find.</param>
		/// <returns>The found <see cref="SongPart"/> or <c>null</c> if there was no part with specified name.</returns>
		public SongPart FindPartByName(string name)
		{
			return (from p in this.Parts where p.Name == name select p).SingleOrDefault();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Song"/> class.
		/// </summary>
		/// <param name="filename">The file to load.</param>
		/// <param name="metadataOnly">If set to <c>true</c> load metadata (title and backgrounds) only.</param>
		public Song(string filename, bool metadataOnly = false) : base(filename)
		{
			if (UndoKey == null)
				Init();

			if (!metadataOnly)
				Load();
		}

		/// <summary>
		/// Initializes some attributes.
		/// </summary>
		private void Init()
		{
			UndoKey = new Undo.UndoKey();
			Parts = new ObservableCollection<SongPart>();
			Sources = new ObservableCollection<SongSource>();
			Order = new ObservableCollection<SongPartReference>();
			Backgrounds = new ObservableCollection<SongBackground>();
		}

		/// <summary>
		/// Loads the media object from the file specified in the <see cref="File"/> field into memory.
		/// This is always called before the control panel and/or presentation is shown.
		/// Use <see cref="MediaManager.LoadMedia"/> to call this safely.
		/// </summary>
		public override void Load()
		{
			LoadPowerpraise(new FileInfo(this.File));
		}

		/// <summary>
		/// Only loads title and backgrounds (for icon).
		/// </summary>
		/// <param name="filename">The file to load.</param>
		protected override void LoadMetadata(string filename)
		{
			base.LoadMetadata(filename);

			if (UndoKey == null)
				Init();

			FileInfo file = new FileInfo(filename);
			if (file.Extension == ".ppl")
			{
				LoadPowerpraise(file, true);
			}
			else
			{
				throw new Exception("Invalid song format");
			}
		}

		/// <summary>
		/// Removes a part from the song (can be undone).
		/// </summary>
		/// <param name="part">The part to remove.</param>
		public void RemovePart(SongPart part)
		{
			int i = Parts.IndexOf(part);
			SongPartReference[] backup = Order.ToArray();

			Action redo = () =>
			{
				bool notify = false;
				SongPartReference pRef;

				while ((pRef = Order.Where(partRef => partRef.Part == part).FirstOrDefault()) != null)
				{
					notify = true;
					Order.Remove(pRef);
				}

				if (notify)
					OnPropertyChanged("PartOrder");

				Parts.Remove(part);
			};

			Action undo = () =>
			{
				if (i == Parts.Count)
					i--;

				Parts.Insert(i, part);
				SetOrder(backup);
				OnPropertyChanged("Order");
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "RemovePart");

			redo();
		}

		/// <summary>
		/// Adds a part to the song (can be undone).
		/// </summary>
		/// <param name="part">The part to add.</param>
		public void AddPart(SongPart part)
		{
			Action undo = () =>
			{
				Parts.Remove(part);
			};

			Action redo = () =>
			{
				Parts.Add(part);
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "AddPart");

			redo();
		}

		/// <summary>
		/// Adds a new part with an empty slide to the song (can be undone).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The added part.</returns>
		public SongPart AddPart(string name)
		{
			SongPart newPart;
			using (Undo.ChangeFactory.Batch(this, "AddPart"))
			{
				newPart = new SongPart(this, name, new SongSlide[] { new SongSlide(this) });
				AddPart(newPart);
			}

			return newPart;
		}

		/// <summary>
		/// Moves a part to the index of another one in the song structure (can be undone).
		/// </summary>
		/// <param name="part">The part to move.</param>
		/// <param name="target">
		/// The target part position where it will be moved to.
		/// If it was before the target, move it directly after the target,
		/// otherwise move it directly before the target.
		/// </param>
		public void MovePart(SongPart part, SongPart target)
		{
			if (part == target)
				return;

			int originalIndex = Parts.IndexOf(part);
			int newIndex = Parts.IndexOf(target);

			Action redo = () =>
			{
				Parts.Move(originalIndex, newIndex);
			};

			Action undo = () =>
			{
				Parts.Move(newIndex, originalIndex);
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "MovePart");

			redo();
		}

		/// <summary>
		/// Copies a specified part and inserts the copy at the index of a target part.
		/// </summary>
		/// <param name="source">The part to copy.</param>
		/// <param name="name">The name that will be given to the copy.</param>
		/// <param name="target">The target position.</param>
		/// <returns>The created copy.</returns>
		public SongPart CopyPart(SongPart source, string name, SongPart target)
		{
			SongPart newPart;
			using (Undo.ChangeFactory.Batch(this, "CopyPart"))
			{
				newPart = source.Copy(name);
				AddPart(newPart);
				MovePart(newPart, target);
			}
			return newPart;
		}

		/// <summary>
		/// Moves a slide to another part (can be undone).
		/// </summary>
		/// <param name="slide">The slide to move.</param>
		/// <param name="target">The target part.</param>
		public void MoveSlide(SongSlide slide, SongPart target)
		{
			var part = FindPartWithSlide(slide);

			using (Undo.ChangeFactory.Batch(this, "MoveSlide"))
			{
				part.RemoveSlide(slide);
				target.AddSlide(slide);
			}
		}

		/// <summary>
		/// Moves a slide to the position directly after another slide (can be undone).
		/// </summary>
		/// <param name="slide">The slide to move.</param>
		/// <param name="target">The target slide.</param>
		public void MoveSlideAfter(SongSlide slide, SongSlide target)
		{
			if (slide == target)
				return;

			var part = FindPartWithSlide(slide);
			var targetPart = FindPartWithSlide(target);

			using (Undo.ChangeFactory.Batch(this, "MoveSlide"))
			{
				part.RemoveSlide(slide);
				targetPart.InsertSlideAfter(slide, target);
			}
		}

		/// <summary>
		/// Copies the specified slide into a given part.
		/// The copy will be appended to the part's slide list
		/// (can be undone).
		/// </summary>
		/// <param name="slide">The slide to copy.</param>
		/// <param name="target">The part where the copy will be inserted.</param>
		/// <returns>The copy.</returns>
		public SongSlide CopySlide(SongSlide slide, SongPart target)
		{
			SongSlide s;
			var part = FindPartWithSlide(slide);

			using (Undo.ChangeFactory.Batch(this, "CopySlide"))
			{
				s = slide.Copy();
				target.AddSlide(s);
			}

			return s;
		}

		/// <summary>
		/// Copies a slide and inserts it after another slide (can be undone).
		/// </summary>
		/// <param name="slide">The slide to copy</param>
		/// <param name="target">The slide after which the copy will be inserted.</param>
		/// <returns>The copy.</returns>
		public SongSlide CopySlideAfter(SongSlide slide, SongSlide target)
		{
			SongSlide s;
			var part = FindPartWithSlide(slide);
			var targetPart = FindPartWithSlide(target);

			using (Undo.ChangeFactory.Batch(this, "CopySlideAfter"))
			{
				s = slide.Copy();
				targetPart.InsertSlideAfter(s, target);
			}

			return s;
		}

		/// <summary>
		/// Adds a part to the part order (can be undone).
		/// </summary>
		/// <param name="part">The part to add.</param>
		/// <param name="index">The index where to insert it in the order. If omitted, append at the end.</param>
		/// <returns>A reference to the part in the order.</returns>
		public SongPartReference AddPartToOrder(SongPart part, int index = -1)
		{
			if (Parts.IndexOf(part) < 0)
				throw new ArgumentException("part has not been added to this song");

			SongPartReference reference = null;

			Action redo = () =>
			{
				if (index < 0)
					index = Order.Count;

				reference = new SongPartReference(part);
				Order.Insert(index, reference);
			};
			Action undo = () =>
			{
				Order.RemoveAt(index);
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "AddPartToOrder");

			redo();

			return reference;
		}

		/// <summary>
		/// Moves a part in the order (can be undone).
		/// </summary>
		/// <param name="reference">The reference in the order to the part to move.</param>
		/// <param name="target">The target index.</param>
		public void MovePartInOrder(SongPartReference reference, int target)
		{
			if (target >= Order.Count)
				target = Order.Count - 1;

			int index = Order.IndexOf(reference);

			Action redo = () =>
			{
				Order.RemoveAt(index);
				Order.Insert(target, reference);
			};
			Action undo = () =>
			{
				Order.RemoveAt(target);
				Order.Insert(index, reference);
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "MovePartInOrder");

			redo();
		}

		/// <summary>
		/// Removes a part from the order (can be undone).
		/// </summary>
		/// <param name="reference">The part to remove, identified by a <see cref="SongPartReference"/>.</param>
		public void RemovePartFromOrder(SongPartReference reference)
		{
			int index = Order.IndexOf(reference);

			Action redo = () =>
			{
				Order.RemoveAt(index);
				//OnNotifyPropertyChanged("PartOrder");
			};

			Action undo = () =>
			{
				Order.Insert(index, reference);
				//OnNotifyPropertyChanged("PartOrder");
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "RemovePartFromOrder");

			redo();
		}

		/// <summary>
		/// Finds the part that contains a given slide.
		/// </summary>
		/// <param name="slide">The slide. Slide instances must always be unique in a song structure.</param>
		/// <returns>The part that contains the slide.</returns>
		public SongPart FindPartWithSlide(SongSlide slide)
		{
			return Parts.Where(p => p.Slides.Contains(slide)).SingleOrDefault();
		}

		
		/// <summary>
		/// Adds a new source from a string.
		/// </summary>
		/// <param name="source">The string.</param>
		/// <returns>The added <see cref="SongSource"/> object.</returns>
		public SongSource AddSource(string source)
		{
			var src = SongSource.Parse(source, this);
			Sources.Add(src);
			return src;
		}

		/// <summary>
		/// Adds the given background to the song's background list if it doesn't already exist (can be undone).
		/// </summary>
		/// <param name="bg"></param>
		/// <returns>The index of the new background.</returns>
		public int AddBackground(SongBackground bg)
		{
			bool contains = Backgrounds.Contains(bg);

			Action redo = () =>
			{
				if (!contains)
					Backgrounds.Add(bg);
			};

			Action undo = () =>
			{
				if (!contains)
					Backgrounds.Remove(bg);
			};

			Undo.ChangeFactory.OnChanging(this, undo, redo, "AddBackground");

			if (contains)
			{
				return Backgrounds.IndexOf(bg);
			}
			else
			{
				Backgrounds.Add(bg);
				return Backgrounds.Count - 1;
			}
		}

		/// <summary>
		/// Removes unreferenced backgrounds and updates the the slide accordingly.
		/// </summary>
		public void CleanBackgrounds()
		{
			Dictionary<int, int> indices = new Dictionary<int, int>();
			var backup = Backgrounds.ToArray();

			// collect all backgrounds in use
			foreach (SongPart part in Parts)
			{
				foreach (SongSlide slide in part.Slides)
				{
					indices[slide.BackgroundIndex] = 0;
				}
			}

			Action redo = () =>
			{
				// remove the ones not in use and store offsets for the rest
				int offset = 0;
				for (int i = 0; i < Backgrounds.Count; )
				{
					if (!indices.ContainsKey(i - offset)) // background is not in use
					{
						Backgrounds.RemoveAt(i);
						offset--;
					}
					else
					{
						indices[i - offset] = offset;
						i++;
					}
				}
			};

			Action undo = () =>
			{
				Backgrounds.Clear();
				foreach (var back in backup)
				{
					Backgrounds.Add(back);
				}
			};

			using (Undo.ChangeFactory.Batch(this, "CleanBackgrounds"))
			{
				redo();

				// update every slide's background index
				foreach (SongPart part in Parts)
				{
					foreach (SongSlide slide in part.Slides)
					{
						slide.BackgroundIndex = slide.BackgroundIndex + indices[slide.BackgroundIndex];
					}
				}

				Undo.ChangeFactory.OnChanging(this, undo, redo, "CleanBackgrounds");
			}
		}

		/// <summary>
		/// Sets the background of every slide in the song.
		/// </summary>
		/// <param name="bg">The background to use.</param>
		public void SetBackground(SongBackground bg)
		{
			var backup = Backgrounds.ToArray();
			Action redo = () =>
			{
				Backgrounds.Clear();
				Backgrounds.Add(bg);

			};

			Action undo = () =>
			{
				Backgrounds.Clear();
				foreach (var back in backup)
				{
					Backgrounds.Add(back);
				}
			};

			using (Undo.ChangeFactory.Batch(this, "SetBackground"))
			{
				Backgrounds.Clear();
				Backgrounds.Add(bg);

				foreach (var part in this.Parts)
				{
					foreach (var slide in part.Slides)
						slide.BackgroundIndex = 0;
				}

				Undo.ChangeFactory.OnChanging(this, undo, redo, "SetBackground");
			}
		}

		/// <summary>
		/// Sets the part order using the part's names. Parts with these names must already exist in the parts list.
		/// </summary>
		/// <param name="partNames">The names of the parts in the order they should appear.</param>
		public void SetOrder(IEnumerable<string> partNames)
		{
			Order.Clear();
			foreach (var n in partNames)
			{
				Order.Add(new SongPartReference(this, n));
			}
		}

		/// <summary>
		/// Sets the part order using the part's names. Parts with these names must already exist in the parts list.
		/// </summary>
		/// <param name="partNames">The names of the parts in the order they should appear.</param>
		public void SetOrder(IEnumerable<SongPartReference> partReferences)
		{
			Order.Clear();
			foreach (var r in partReferences)
			{
				Order.Add(r);
			}
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public Song Root
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Gets the title of this media object.
		/// </summary>
		public override string Title
		{
			get
			{
				return SongTitle;
			}
		}

		#endregion

		#region Powerpraise compatibility

		/// <summary>
		/// Loads a song from a Powerpraise XML file (ppl)
		/// </summary>
		/// <param name="file">The file to load.</param>
		/// <param name="metadataOnly">If set to <c>true</c> load metadata (title and backgrounds) only.</param>
		private void LoadPowerpraise(FileInfo file, bool metadataOnly = false)
		{
			XDocument doc = XDocument.Load(file.FullName);
			XElement root = doc.Element("ppl");
			this.SongTitle = root.Element("general").Element("title").Value;

			var formatting = root.Element("formatting");

			// reset in case it has already been loaded
			this.Backgrounds.Clear(); // this is needed, because the indices must be correct
			this.Sources.Clear();
			this.Parts.Clear();

			var video = root.Element("formatting").Element("background").Attribute("video");
			if (video != null)
			{
				this.Backgrounds.Add(new SongBackground(video.Value, true));
			}
			else
			{
				foreach (var bg in root.Element("formatting").Element("background").Elements("file"))
				{
					this.Backgrounds.Add(LoadPowerpraiseBackground(bg.Value));
				}
			}
			
			if (!metadataOnly)
			{
				this.Formatting = new SongFormatting
				{
					MainText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("maintext")),
					TranslationText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("translationtext")),
					SourceText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("sourcetext")),
					CopyrightText = LoadPowerpraiseTextFormatting(formatting.Element("font").Element("copyrighttext")),
					TextLineSpacing = int.Parse(formatting.Element("linespacing").Element("main").Value),
					TranslationLineSpacing = int.Parse(formatting.Element("linespacing").Element("translation").Value),
					SourceBorderRight = int.Parse(formatting.Element("borders").Element("sourceright").Value),
					SourceBorderTop = int.Parse(formatting.Element("borders").Element("sourcetop").Value),
					CopyrightBorderBottom = int.Parse(formatting.Element("borders").Element("copyrightbottom").Value),
					HorizontalOrientation = (HorizontalTextOrientation)Enum.Parse(typeof(HorizontalTextOrientation), formatting.Element("textorientation").Element("horizontal").Value, true),
					VerticalOrientation = (VerticalTextOrientation)Enum.Parse(typeof(VerticalTextOrientation), formatting.Element("textorientation").Element("vertical").Value, true),
					BorderBottom = int.Parse(formatting.Element("borders").Element("mainbottom").Value),
					BorderTop = int.Parse(formatting.Element("borders").Element("maintop").Value),
					BorderLeft = int.Parse(formatting.Element("borders").Element("mainleft").Value),
					BorderRight = int.Parse(formatting.Element("borders").Element("mainright").Value),
					IsOutlineEnabled = bool.Parse(formatting.Element("font").Element("outline").Element("enabled").Value),
					OutlineColor = ParsePowerpraiseColor(formatting.Element("font").Element("outline").Element("color").Value),
					IsShadowEnabled = bool.Parse(formatting.Element("font").Element("shadow").Element("enabled").Value),
					ShadowColor = ParsePowerpraiseColor(formatting.Element("font").Element("shadow").Element("color").Value),
					ShadowDirection = int.Parse(formatting.Element("font").Element("shadow").Element("direction").Value),
					TranslationPosition = formatting.Element("textorientation").Element("transpos") != null ?
						(TranslationPosition)Enum.Parse(typeof(TranslationPosition), formatting.Element("textorientation").Element("transpos").Value, true) : TranslationPosition.Inline,
					CopyrightDisplayPosition = (MetadataDisplayPosition)Enum.Parse(typeof(MetadataDisplayPosition), root.Element("information").Element("copyright").Element("position").Value, true),
					SourceDisplayPosition = (MetadataDisplayPosition)Enum.Parse(typeof(MetadataDisplayPosition), root.Element("information").Element("source").Element("position").Value, true)
				};

				if (root.Element("general").Element("category") != null)
					this.Category = root.Element("general").Element("category").Value;
				if (root.Element("general").Element("language") != null)
					this.Language = root.Element("general").Element("language").Value;
				if (root.Element("general").Element("comment") != null)
					Comment = root.Element("general").Element("comment").Value;
				else
					Comment = String.Empty;

				foreach (var part in root.Element("songtext").Elements("part"))
				{
					this.Parts.Add(new SongPart(this,
									part.Attribute("caption").Value,
									from slide in part.Elements("slide") select new SongSlide(this)
									{
										Text = String.Join("\n", slide.Elements("line").Select(line => line.Value).ToArray())/*.Trim()*/,
										Translation = String.Join("\n", slide.Elements("translation").Select(line => line.Value).ToArray())/*.Trim()*/,
										BackgroundIndex = slide.Attribute("backgroundnr") != null ? int.Parse(slide.Attribute("backgroundnr").Value) : 0,
										Size = slide.Attribute("mainsize") != null ? int.Parse(slide.Attribute("mainsize").Value) : Formatting.MainText.Size
									}
								));
				}

				this.SetOrder(from item in root.Element("order").Elements("item") select item.Value);

				this.Copyright = String.Join("\n", root.Element("information").Element("copyright").Element("text").Elements("line").Select(line => line.Value).ToArray());

				this.AddSource(String.Join("\n", root.Element("information").Element("source").Element("text").Elements("line").Select(line => line.Value)));
			}
		}

		/// <summary>
		/// Saves the song to a Powerpraise XML file (ppl version 3.0).
		/// TODO: move to a separate class
		/// </summary>
		/// <param name="fileName">The file to save to.</param>
		public void SavePowerpraise(string fileName)
		{
			XDocument doc = new XDocument(new XDeclaration("1.0","ISO-8859-1","yes"));
			XElement root = new XElement("ppl", new XAttribute("version", "3.0"),
				new XElement("general",
					new XElement("title", this.SongTitle),
					new XElement("category",this.Category),
					new XElement("language", this.Language),
					String.IsNullOrEmpty(this.Comment) ? null : new XElement("comment", this.Comment)),
				new XElement("songtext",
					from part in this.Parts select new XElement("part",
						new XAttribute("caption", part.Name),
						from slide in part.Slides select new XElement("slide",
							new XAttribute("mainsize", slide.Size),
							new XAttribute("backgroundnr", slide.BackgroundIndex),
							!String.IsNullOrEmpty(slide.Text) ?
								from line in slide.Text.Split('\n') select new XElement("line", RemoveLineBreaks(line)) : null,
							!String.IsNullOrEmpty(slide.Translation) ?
								from translationline in slide.Translation.Split('\n') select new XElement("translation", RemoveLineBreaks(translationline)) : null
						)
					)
				),
				new XElement("order",
					from item in this.Order select new XElement("item", item.Part.Name)
				),
				new XElement("information",
					new XElement("copyright",
						new XElement("position", this.Formatting.CopyrightDisplayPosition.ToString().ToLower()),
						new XElement("text",
							!String.IsNullOrEmpty(this.Copyright) ?
								from line in this.Copyright.Split('\n') select new XElement("line", line) : null
						)
					),
					new XElement("source",
						new XElement("position", this.Formatting.SourceDisplayPosition.ToString().ToLower()),
						new XElement("text",
							this.Sources.Count > 0 && !String.IsNullOrEmpty(this.Sources[0].ToString()) ?
								new XElement("line", this.Sources[0].ToString()) : null
						)
					)
				),
				new XElement("formatting",
					new XElement("font",
						SavePowerpraiseTextFormatting(this.Formatting.MainText, "maintext"),
						SavePowerpraiseTextFormatting(this.Formatting.TranslationText, "translationtext"),
						SavePowerpraiseTextFormatting(this.Formatting.CopyrightText, "copyrighttext"),
						SavePowerpraiseTextFormatting(this.Formatting.SourceText, "sourcetext"),
						new XElement("outline",
							new XElement("enabled", this.Formatting.IsOutlineEnabled.ToString().ToLower()),
							new XElement("color", CreatePowerpraiseColor(this.Formatting.OutlineColor))
						),
						new XElement("shadow",
							new XElement("enabled", this.Formatting.IsShadowEnabled.ToString().ToLower()),
							new XElement("color", CreatePowerpraiseColor(this.Formatting.ShadowColor)),
							new XElement("direction", this.Formatting.ShadowDirection)
						)
					),
					new XElement("background",
						this.VideoBackground != null ?
						new object[] {
							new XAttribute("video", this.VideoBackground.FilePath),
							new XElement("file", "none") // for backwards compatibility
						} :
						(from bg in this.Backgrounds select new XElement("file", SavePowerpraiseBackground(bg))).ToArray()
					),
					new XElement("linespacing",
						new XElement("main", this.Formatting.TextLineSpacing),
						new XElement("translation", this.Formatting.TranslationLineSpacing)
					),
					new XElement("textorientation",
						new XElement("horizontal", this.Formatting.HorizontalOrientation.ToString().ToLower()),
						new XElement("vertical", this.Formatting.VerticalOrientation.ToString().ToLower()),
						new XElement("transpos", this.Formatting.TranslationPosition.ToString().ToLower())
					),
					new XElement("borders",
						new XElement("mainleft", this.Formatting.BorderLeft),
						new XElement("maintop", this.Formatting.BorderTop),
						new XElement("mainright", this.Formatting.BorderRight),
						new XElement("mainbottom", this.Formatting.BorderBottom),
						new XElement("copyrightbottom", this.Formatting.CopyrightBorderBottom),
						new XElement("sourcetop", this.Formatting.SourceBorderTop),
						new XElement("sourceright", this.Formatting.SourceBorderRight)
					)
				)
			);
			doc.Add(new XComment("This file was written using WordsLive"));
			doc.Add(root);

			StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("iso-8859-1"));
			doc.Save(writer);
			writer.Close();
		}

		/// <summary>
		/// Removes all line break characters (\n and \r).
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>The processed input.</returns>
		private string RemoveLineBreaks(string input)
		{
			return input.Replace("\n", "").Replace("\r", "");
		}

		/// <summary>
		/// Helper method to generate an XML object from a <see cref="SongTextFormatting"/> object.
		/// </summary>
		/// <param name="formatting">The formatting object.</param>
		/// <param name="elementName">The element name to generate.</param>
		/// <returns>The generated XML.</returns>
		private static XElement SavePowerpraiseTextFormatting(SongTextFormatting formatting, string elementName)
		{
			return new XElement(elementName,
				new XElement("name", formatting.Name),
				new XElement("size", formatting.Size),
				new XElement("bold", formatting.Bold.ToString().ToLower()),
				new XElement("italic", formatting.Italic.ToString().ToLower()),
				new XElement("color", CreatePowerpraiseColor(formatting.Color)),
				new XElement("outline", formatting.Outline),
				new XElement("shadow", formatting.Shadow)
			);
		}

		/// <summary>
		/// Helper method to load a <see cref="SongTextFormatting"/> object from an XML object.
		/// </summary>
		/// <param name="element">The XML element.</param>
		/// <returns>The loaded formatting object.</returns>
		private static SongTextFormatting LoadPowerpraiseTextFormatting(XElement element)
		{
			return new SongTextFormatting
			{
				Name = element.Element("name").Value,
				Size = int.Parse(element.Element("size").Value),
				Bold = bool.Parse(element.Element("bold").Value),
				Italic = bool.Parse(element.Element("italic").Value),
				Color = ParsePowerpraiseColor(element.Element("color").Value),
				Outline = int.Parse(element.Element("outline").Value),
				Shadow = int.Parse(element.Element("shadow").Value)
			};
		}

		/// <summary>
		/// Helper method to save a <see cref="SongBackground"/> object to XML.
		/// </summary>
		/// <param name="background">The background object.</param>
		/// <returns>The path if the background is an image, otherwise the encoded color.</returns>
		private static string SavePowerpraiseBackground(SongBackground background)
		{
			if (background.IsFile)
				return background.FilePath;
			else
				return CreatePowerpraiseColor(background.Color).ToString();
		}

		/// <summary>
		/// Helper method to load a <see cref="SongBackground"/> from XML (either an image path or a color).
		/// </summary>
		/// <param name="background">The string encoding of the background object.</param>
		/// <returns>The loaded background object.</returns>
		private static SongBackground LoadPowerpraiseBackground(string background)
		{
			SongBackground bg;
			if (background == "none")
			{
				bg = new SongBackground(Color.Black);
			}
			else if (Regex.IsMatch(background, @"^\d{1,8}$"))
			{
				bg = new SongBackground(ParsePowerpraiseColor(background));
			}
			else
			{
				bg = new SongBackground(background, false);
			}
			return bg;
		}

		/// <summary>
		/// Helper method to create the color encoding used in Powerpraise XML.
		/// </summary>
		/// <param name="color">The color to encode.</param>
		/// <returns>The encoded color.</returns>
		private static int CreatePowerpraiseColor(Color color)
		{
			return color.R | (color.G << 8) | (color.B << 16);
		}

		/// <summary>
		/// Helper method to parse a color encoded in Powerpraise XML.
		/// </summary>
		/// <param name="color">The string containing the encoded color.</param>
		/// <returns>The color.</returns>
		private static Color ParsePowerpraiseColor(string color)
		{
			int col = int.Parse(color);
			return Color.FromArgb(col & 255, (col >> 8) & 255, (col >> 16) & 255);
		}

		#endregion
	}
}