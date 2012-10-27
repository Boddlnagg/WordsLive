using System;
using System.Collections.Generic;
using System.Linq;
using MonitoredUndo;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.Chords;
using System.Collections.ObjectModel;

namespace WordsLive.Editor
{
	public class SongPartWrapper
	{
		public SongNodePart Content { get; set; }
	}

	public class SongNodeRoot : SongNode
	{
		private List<SongNodePart> partNodes = new List<SongNodePart>();
		private SongNodeLanguage languageNode;
		private SongNodeMetadata categoryNode;
		private SongNodeMetadata copyrightNode;
		private SongNodeSource sourceNode;

		public IEnumerable<SongPartWrapper> PartOrder
		{
			get
			{
				foreach (var partRef in Song.Order)
				{
					foreach(var part in this.partNodes)
					{
						if (part.Title == partRef.Part.Name)
						{
							yield return new SongPartWrapper {Content = part};
							break;
						}
					}
				}
			}
		}

		public Song Song
		{
			get;
			private set;
		}

		public string LanguageCode
		{
			get
			{
				return LanguageCodeFromName(Song.Language);
			}
		}

		public string TranslationLanguageCode
		{
			get
			{
				return LanguageCodeFromName(Song.TranslationLanguage);
			}
		}

		private string LanguageCodeFromName(string name)
		{
			if (String.IsNullOrEmpty(name))
				return String.Empty;

			int index = name.IndexOfAny(new char[] { '/', ' ', '(' });
			if (index > 0)
			{
				name = name.Substring(0, index);
			}

			switch (name.ToLower())
			{
				case "deutsch":
				case "german":
					return "de";
				case "schweizerdeutsch":
					return "de-ch";
				case "englisch":
				case "english":
					return "en";
				case "italienisch":
				case "italiano":
				case "italian":
					return "it";
				case "französisch":
				case "francais":
				case "french":
					return "fr";
				case "spanisch":
				case "espanol":
				case "spanish":
					return "es";
				default:
					return String.Empty;
			}
		}

		public override string IconUri
		{
			get { return "/WordsLive;component/Artwork/Small_Song.png"; }
		}

		public SongNodeRoot(Song song) : base(null)
		{
			this.Song = song;

			this.Title = song.SongTitle;

			foreach (SongPart part in song.Parts)
			{
				partNodes.Add(new SongNodePart(Root, this.Song, part));
			}

			sourceNode = new SongNodeSource(Root, song.Sources[0]);
			copyrightNode = new SongNodeCopyright(Root);
			languageNode = new SongNodeLanguage(Root);
			languageNode.PropertyChanged += (sender, args) =>
				{
					if (args.PropertyName == "Language")
						this.OnNotifyPropertyChanged("LanguageCode");
					else if (args.PropertyName == "TranslationLanguage")
						this.OnNotifyPropertyChanged("TranslationLanguageCode");
				};
			categoryNode = new SongNodeCategory(Root);

			this.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Title")
				{
					this.Song.SongTitle = this.Title;
				}
			};
		}

		// don't forget to call this when modifying parts
		private void UpdateParts()
		{
			Song.Parts = new ObservableCollection<SongPart>(from p in partNodes select p.Part);
			OnNotifyPropertyChanged("Children");
			OnNotifyPropertyChanged("Parts");
		}

		public void RemovePart(SongNodePart part)
		{
			int i = partNodes.IndexOf(part);
			SongPartReference[] backup = Song.Order.ToArray();

			Action redo = () =>
			{
				bool notify = false;
				while(Song.Order.Contains(new SongPartReference(part.Part)))
				{
					notify = true;
					Song.Order.Remove(new SongPartReference(part.Part));
				}

				if (notify)
					OnNotifyPropertyChanged("PartOrder");

				partNodes.Remove(part);
				UpdateParts();
			};

			Action undo = () =>
			{
				partNodes.Insert(i, part);
				Song.Order = new List<SongPartReference>(backup);
				OnNotifyPropertyChanged("PartOrder");
				UpdateParts();
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "Parts"));
			UndoService.Current[this].AddChange(ch, "RemovePart");

			redo();
		}

		public void AddPart(SongNodePart part)
		{
			Action undo = () =>
			{
				partNodes.Remove(part);
				UpdateParts();
			};

			Action redo = () =>
			{
				partNodes.Add(part);
				UpdateParts();
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "Parts"));
			UndoService.Current[this].AddChange(ch, "AddPart");

			redo();
		}

		public void MovePart(SongNodePart part, SongNodePart target)
		{
			if (part == target)
				return;

			int originalIndex = partNodes.IndexOf(part);
			int newIndex = partNodes.IndexOf(target);

			Action redo = () =>
			{
				partNodes.Remove(part);
				partNodes.Insert(newIndex, part);
				UpdateParts();
			};

			Action undo = () =>
			{
				partNodes.Remove(part);
				partNodes.Insert(originalIndex, part);
				UpdateParts();
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "Parts"));
			UndoService.Current[this].AddChange(ch, "MovePart");

			redo();
		}

		public void MoveSlide(SongNodeSlide slide, SongNodePart target)
		{
			var part = FindPartWithSlide(slide);

			using (new UndoBatch(this, "MoveSlide", false))
			{
				part.RemoveSlide(slide);
				target.AddSlide(slide);
			}
		}

		public void MoveSlide(SongNodeSlide slide, SongNodeSlide target)
		{
			if (slide == target)
				return;

			var part = FindPartWithSlide(slide);
			var targetPart = FindPartWithSlide(target);

			using (new UndoBatch(this, "MoveSlide", false))
			{
				part.RemoveSlide(slide);
				targetPart.InsertSlideAfter(slide, target);
			}
		}

		public void CopySlide(SongNodeSlide slide, SongNodePart target)
		{
			SongNodeSlide s;
			var part = FindPartWithSlide(slide);

			using (new UndoBatch(this, "CopySlide", false))
			{
				s = slide.Copy();
				target.AddSlide(s);
			}
		}

		public void CopySlide(SongNodeSlide slide, SongNodeSlide target)
		{
			SongNodeSlide s;
			var part = FindPartWithSlide(slide);
			var targetPart = FindPartWithSlide(target);

			using (new UndoBatch(this, "CopySlide", false))
			{
				s = slide.Copy();
				targetPart.InsertSlideAfter(s, target);
			}
		}

		public void AddPartToOrder(SongNodePart part, int index = -1)
		{
			Action redo = () =>
			{
				if (index < 0)
					index = Song.Order.Count;

				Song.Order.Insert(index, new SongPartReference(part.Part));

				OnNotifyPropertyChanged("PartOrder");
			};
			Action undo = () =>
			{
				Song.Order.RemoveAt(index);
				OnNotifyPropertyChanged("PartOrder");
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "PartOrder"));
			UndoService.Current[this].AddChange(ch, "AddPartToOrder");

			redo();
		}

		public void MovePartInOrder(int index, int newIndex)
		{
			if (newIndex >= Song.Order.Count)
					newIndex = Song.Order.Count - 1;

				var reference = Song.Order[index];

			Action redo = () =>
			{
				Song.Order.RemoveAt(index);
				Song.Order.Insert(newIndex, reference);
				OnNotifyPropertyChanged("PartOrder");
			};
			Action undo = () =>
			{
				Song.Order.RemoveAt(newIndex);
				Song.Order.Insert(index, reference);
				OnNotifyPropertyChanged("PartOrder");
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "PartOrder"));
			UndoService.Current[this].AddChange(ch, "MovePartInOrder");

			redo();
		}

		public void RemovePartFromOrder(int index)
		{
			var reference = Song.Order[index];
			Action redo = () =>
			{
				Song.Order.RemoveAt(index);
				OnNotifyPropertyChanged("PartOrder");
			};

			Action undo = () =>
			{
				Song.Order.Insert(index, reference);
				OnNotifyPropertyChanged("PartOrder");
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "PartOrder"));
			UndoService.Current[this].AddChange(ch, "RemovePartFromOrder");

			redo();
		}

		public SongNodePart FindPartWithSlide(SongNodeSlide slide)
		{
			foreach (var p in partNodes)
			{
				if (p.Children.Contains(slide))
				{
					return p;
				}
			}

			return null;
		}

		public void RemoveSlide(SongNodeSlide slide)
		{
			var part = FindPartWithSlide(slide);

			if (part == null)
				throw new InvalidOperationException("Slide not part of song");

			part.RemoveSlide(slide);
		}

		public IEnumerable<SongNodePart> Parts
		{
			get
			{
				return partNodes;
			}
		}

		public IEnumerable<SongNode> Children
		{
			get
			{
				foreach (SongNodePart part in partNodes)
				{
					yield return part;
				}
				yield return sourceNode;
				yield return copyrightNode;
				yield return languageNode;
				yield return categoryNode;
			}
		}

		// TODO: this is currently never used, because the unicode characters are not saved correctly in the .ppl
		public void PrettyPrintChords()
		{
			using (new UndoBatch(this, "PrettyPrintChords", false))
			{
				foreach (var part in Parts)
				{
					foreach (var slide in part.Children)
					{
						slide.Text = Chords.PrettyPrint(slide.Text);
					}
				}
			}
		}

		public void TransposeChords(Key originalKey, int amount)
		{
			using (new UndoBatch(this, "TransposeChords", false))
			{
				foreach (var part in Parts)
				{
					foreach (var slide in part.Children)
					{
						slide.Text = Chords.Transpose(slide.Text, originalKey, amount);
					}
				}
			}
		}

		public void RemoveAllChords()
		{
			using (new UndoBatch(this, "RemoveAllChords", false))
			{
				foreach (var part in Parts)
				{
					foreach (var slide in part.Children)
					{
						slide.Text = Chords.RemoveAll(slide.Text);
					}
				}
			}
		}

		/// <summary>
		/// Adds the given background to the song's background list if it doesn't already exist.
		/// </summary>
		/// <param name="bg"></param>
		/// <returns>The index of the new background.</returns>
		public int AddBackground(SongBackground bg)
		{
			bool contains = Song.Backgrounds.Contains(bg);

			Action redo = () =>
			{
				if (!contains)
					Song.Backgrounds.Add(bg);
			};

			Action undo = () =>
			{
				if (!contains)
					Song.Backgrounds.Remove(bg);
			};

			var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "Song.Backgrounds"));
			UndoService.Current[this].AddChange(ch, "AddBackground");

			if (contains)
			{
				return Song.Backgrounds.IndexOf(bg);
			}
			else
			{
				Song.Backgrounds.Add(bg);
				return Song.Backgrounds.Count - 1;
			}
		}

		/// <summary>
		/// Sets the background of every slide in the song
		/// </summary>
		/// <param name="bg"></param>
		public void SetBackground(SongBackground bg)
		{
			var backup = Song.Backgrounds.ToArray();
			Action redo = () =>
			{
				Song.Backgrounds.Clear();
				Song.Backgrounds.Add(bg);
				
			};

			Action undo = () =>
			{
				Song.Backgrounds = new List<SongBackground>(backup);
			};

			using (new UndoBatch(this, "SetBackground", false))
			{
				Song.Backgrounds.Clear();
				Song.Backgrounds.Add(bg);

				foreach (var part in this.Parts)
				{
					foreach (var slide in part.Children)
						slide.BackgroundIndex = 0;
				}

				var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "Song.Backgrounds"));
				UndoService.Current[this].AddChange(ch, "SetBackground");
			}
		}

		/// <summary>
		/// Removes unreferenced backgrounds and updates the the slide accordingly.
		/// </summary>
		public void CleanBackgrounds()
		{
			Dictionary<int, int> indices = new Dictionary<int, int>();
			var backup = Song.Backgrounds.ToArray();

			// collect all backgrounds in use
			foreach (SongNodePart part in Parts)
			{
				foreach (SongNodeSlide slide in part.Children)
				{
					indices[slide.BackgroundIndex] = 0;
				}
			}

			Action redo = () =>
			{
				// remove the ones not in use and store offsets for the rest
				int offset = 0;
				for (int i = 0; i < Song.Backgrounds.Count; )
				{
					if (!indices.ContainsKey(i - offset)) // background is not in use
					{
						Song.Backgrounds.RemoveAt(i);
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
				Song.Backgrounds = new List<SongBackground>(backup);
			};

			using (new UndoBatch(this, "CleanBackgrounds", false))
			{
				redo();

				// update every slide's background index
				foreach (SongNodePart part in Parts)
				{
					foreach (SongNodeSlide slide in part.Children)
					{
						slide.BackgroundIndex = slide.BackgroundIndex + indices[slide.BackgroundIndex];
					}
				}

				var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "Song.Backgrounds"));
				UndoService.Current[this].AddChange(ch, "CleanBackgrounds");
			}
		}
	}
}
