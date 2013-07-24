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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WordsLive.Core.Songs.IO;
using WordsLive.Core.Songs.Storage;
using WordsLive.Core.Songs.Undo;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents a song media object.
	/// </summary>
	public class Song : INotifyPropertyChanged, ISongElement
	{
		private SongUriResolver uriResolver;
		public Uri Uri { get; private set; }

		private string title;
		private string category;
		private string language;
		private string translationLanguage;
		private string comment;
		private string copyright;
		private SongFormatting formatting;

		private bool isModified;

		/// <summary>
		/// Gets a value indicating whether this instance has been modified since the last time it was saved.
		/// </summary>
		[JsonIgnore]
		public bool IsModified
		{
			get
			{
				return isModified;
			}
			private set
			{
				isModified = value;
				OnPropertyChanged("IsModified");
			}
		}

		private bool isImported;

		[JsonIgnore]
		public bool IsImported
		{
			get
			{
				return isImported;
			}
			private set
			{
				isImported = value;
				OnPropertyChanged("IsImported");

				if (value)
					IsModified = true;
			}
		}

		#region Undo/Redo

		private UndoManager undoManager;
		private bool isUndoEnabled = false;

		[JsonIgnore]
		public UndoManager UndoManager
		{
			get
			{
				if (!isUndoEnabled)
					throw new InvalidOperationException("Undo is currently disabled.");

				if (undoManager == null)
				{
					undoManager = new UndoManager(new MonitoredUndo.UndoRoot(this), () => this.IsModified = true);
				}
				
				return undoManager;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether undo is enabled for this song.
		/// This is <c>false</c> by default.
		/// When it has been <c>true</c> and is set to <c>false</c>, the undo/redo stack
		/// is cleared and cannot be restored again.
		/// </summary>
		[JsonIgnore]
		public bool IsUndoEnabled
		{
			get
			{
				return isUndoEnabled;
			}
			set
			{
				if (value != isUndoEnabled)
				{
					if (!value)
					{
						// disable undo -> clear stack
						UndoManager.Root.Clear();
					}

					isUndoEnabled = value;

					if (isUndoEnabled)
					{
						// create UndoManager if it doesn't exist
						var tmp = UndoManager.Root;
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the song title.
		/// </summary>
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value");

				if (value != title)
				{
					Undo.ChangeFactory.OnChanging(this, "Title", title, value);
					title = value;
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
				using (Undo.ChangeFactory.Batch(this, "Formatting"))
				{
					Undo.ChangeFactory.OnChanging(this, "Formatting", formatting == null ? null : formatting.Clone(), value);
					formatting = value;
					var size = formatting.MainText.Size;
					if (formatting.SingleFontSize && (!IsUndoEnabled || !UndoManager.IsUndoingOrRedoing))
					{
						foreach (var part in Parts)
						{
							foreach (var s in part.Slides)
							{
								s.Size = size;
								formatting.MainText.Size = size;
							}
						}
					}
				}
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
		public SongSlide LastSlide
		{
			get
			{
				if (this.Order.Count == 0)
					return null;
				return (from p in this.Parts where p.Name == this.Order[this.Order.Count - 1].Part.Name select p.Slides[p.Slides.Count - 1]).Single();
			}
		}

		private bool hasTranslation = false;

		/// <summary>
		/// Gets a value indicating whether any slide in this song has a translation.
		/// </summary>
		public bool HasTranslation
		{
			get
			{
				return hasTranslation;
			}
		}

		internal void UpdateHasTranslation(bool slideHasTranslation)
		{
			var lastHasTranslation = hasTranslation;

			if (slideHasTranslation)
				hasTranslation = true;
			else
				hasTranslation = Parts.Any(part => part.Slides.Any(slide => slide.HasTranslation));

			if (hasTranslation != lastHasTranslation)
			{
				OnPropertyChanged("HasTranslation");
			}
		}

		private bool hasChords = false;

		/// <summary>
		/// Gets a value indicating whether any slide in this song has chords.
		/// </summary>
		public bool HasChords
		{
			get
			{
				return hasChords;
			}
		}

		internal void UpdateHasChords(bool slideHasChords)
		{ 
			var lastHasChords = hasChords;

			if (slideHasChords)
				hasChords = true;
			else
				hasChords = Chords.Chords.GetChords(Text).Any();

			if (hasChords != lastHasChords)
			{
				OnPropertyChanged("HasChords");
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

		private Song()
		{
			Parts = new ObservableCollection<SongPart>();
			Sources = new ObservableCollection<SongSource>();
			Order = new ObservableCollection<SongPartReference>();
			Backgrounds = new ObservableCollection<SongBackground>();
			this.uriResolver = SongUriResolver.Default;
		}

		public Song(string path) : this(new Uri(new FileInfo(path).FullName)) { }

		public Song(Uri uri) : this(uri, SongUriResolver.Default, new PowerpraiseSongReader()) { }

		public Song(Uri uri, SongUriResolver resolver) : this(uri, resolver, new PowerpraiseSongReader()) { }

		public Song(Uri uri, ISongReader reader) : this(uri, SongUriResolver.Default, reader) { }

		public Song(Uri uri, SongUriResolver resolver, ISongReader reader) : this()
		{
			this.Uri = uri;
			this.uriResolver = resolver;

			using (Stream stream = uriResolver.Get(uri))
			{
				reader.Read(this, stream);
			}

			if (!(reader is PowerpraiseSongReader))
			{
				IsImported = true;
			}
		}

		public static Task<Song> LoadAsync(string path, CancellationToken cancellation = default(CancellationToken))
		{
			return LoadAsync(new Uri(new FileInfo(path).FullName), cancellation);
		}

		public static Task<Song> LoadAsync(Uri uri, CancellationToken cancellation = default(CancellationToken))
		{
			return LoadAsync(uri, SongUriResolver.Default, new PowerpraiseSongReader(), cancellation);
		}

		public static Task<Song> LoadAsync(Uri uri, SongUriResolver resolver, CancellationToken cancellation = default(CancellationToken))
		{
			return LoadAsync(uri, resolver, new PowerpraiseSongReader(), cancellation);
		}

		public static Task<Song> LoadAsync(Uri uri, ISongReader reader, CancellationToken cancellation = default(CancellationToken))
		{
			return LoadAsync(uri, SongUriResolver.Default, reader, cancellation);
		}

		public static async Task<Song> LoadAsync(Uri uri, SongUriResolver resolver, ISongReader reader, CancellationToken cancellation = default(CancellationToken))
		{
			var song = new Song();
			song.Uri = uri;
			song.uriResolver = resolver;

			using (Stream stream = await song.uriResolver.GetAsync(uri, cancellation))
			{
				cancellation.ThrowIfCancellationRequested();
				reader.Read(song, stream);
			}

			if (!(reader is PowerpraiseSongReader))
			{
				song.IsImported = true;
			}

			return song;
		}

		public void LoadTemplate()
		{
			using (Stream stream = SongUriResolver.Default.Get(new Uri(DataManager.SongTemplate.FullName)))
			{
				var reader = new PowerpraiseSongReader();
				reader.Read(this, stream);
			}
		}

		/// <summary>
		/// Creates a new song from a template.
		/// </summary>
		/// <returns></returns>
		public static Song CreateFromTemplate()
		{
			Song song = new Song();
			song.LoadTemplate();
			return song;
		}

		public void Save()
		{
			if (this.Uri == null || IsImported)
				throw new InvalidOperationException("Can't save to unknown source or imported file.");

			using (var ft = uriResolver.Put(this.Uri))
			{
				var writer = new PowerpraiseSongWriter();
				writer.Write(this, ft.Stream);
			}

			IsModified = false;
			IsImported = false;
		}

		public void Save(Uri uri)
		{
			Write(uri, new PowerpraiseSongWriter());
			this.Uri = uri;
			OnPropertyChanged("Uri");

			IsModified = false;
			IsImported = false;
		}

		public void Export(Uri uri, ISongWriter writer)
		{
			Write(uri, writer);
		}

		private void Write(Uri uri, ISongWriter writer)
		{
			if (uri == null)
				throw new ArgumentException("uri");
			if (writer == null)
				throw new ArgumentNullException("writer");

			using (var ft = SongUriResolver.Default.Put(uri))
			{
				writer.Write(this, ft.Stream);
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
		/// Removes unreferenced backgrounds and updates the slides accordingly.
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
		/// <param name="ignoreMissing">If this is not set to true, an exception will be thrown when a part name does not exist;
		/// otherwise, missing parts will be ignored.</param>
		public void SetOrder(IEnumerable<string> partNames, bool ignoreMissing = false)
		{
			Order.Clear();
			foreach (var n in partNames)
			{
				try
				{
					Order.Add(new SongPartReference(this, n));
				}
				catch (ArgumentNullException)
				{
					if (!ignoreMissing)
						throw;
				}
			}
		}

		/// <summary>
		/// Sets the part order using part references.
		/// </summary>
		/// <param name="partNames">The part references to use in the order they should appear.</param>
		public void SetOrder(IEnumerable<SongPartReference> partReferences)
		{
			Order.Clear();
			foreach (var r in partReferences)
			{
				Order.Add(r);
			}
		}

		public bool CheckSingleFontSize()
		{
			if (FirstSlide == null)
				return true; // use Formatting.MainText.Size

			int size = FirstSlide.Size;
			foreach (var part in Parts)
			{
				foreach (var s in part.Slides)
				{
					if (s.Size != size)
						return false;
				}
			}
			Formatting.MainText.Size = size;
			return true;
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		[JsonIgnore]
		public Song Root
		{
			get
			{
				return this;
			}
		}
		#endregion
	}
}