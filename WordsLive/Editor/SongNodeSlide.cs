using System;
using System.Collections.Generic;
using System.Linq;
using MonitoredUndo;
using WordsLive.Core.Songs;
using WordsLive.Core.Songs.Chords;

namespace WordsLive.Editor
{
	public class SongNodeSlide : SongNode
	{
		private SongSlide slide;
		private bool hasTranslation;
		private bool hasChords;

		public override string IconUri
		{
			get
			{
				return "/WordsLive;component/Artwork/Small_Slide.png";
			}
		}

		public SongSlide Slide
		{
			get
			{
				return slide;
			}
		}

		public string Text
		{
			get
			{
				return slide.Text;
			}
			set
			{
				if (value != slide.Text)
				{
					if (value == null)
						value = string.Empty;

					EditorDocument.OnChangingTryMerge(this, "Text", slide.Text, value);
					slide.Text = value;
					HasChords = Chords.GetChords(slide.Text).Any();
					OnNotifyPropertyChanged("Text");
					OnNotifyPropertyChanged("TextWithoutChords");
				}
			}
		}

		public string TextWithoutChords
		{
			get
			{
				return slide.TextWithoutChords;
			}
		}

		public string Translation
		{
			get
			{
				return slide.Translation;
			}
			set
			{
				if (value != slide.Translation)
				{
					EditorDocument.OnChangingTryMerge(this, "Translation", slide.Translation, value);
					slide.Translation = value;
					HasTranslation = !string.IsNullOrEmpty(slide.Translation);
					OnNotifyPropertyChanged("Translation");
				}
			}
		}

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

		public IEnumerable<SongNode> Children
		{
			get
			{
				return null;
			}
		}

		public int FontSize
		{
			get
			{
				return slide.Size;
			}
			private set
			{
				DefaultChangeFactory.OnChanging(this, "FontSize", slide.Size, value);
				slide.Size = value;
				OnNotifyPropertyChanged("FontSize");
			}
		}

		public void ChangeFontSize(int newSize)
		{
			using (new UndoBatch(Root, "ChangeFontSize", false))
			{
				this.FontSize = newSize;
				int oldSize = Root.Song.Formatting.MainText.Size;
				Action undo = () =>
				{
					var formatting = Root.Song.Formatting;
					formatting.MainText.Size = oldSize;
					Root.Song.Formatting = formatting;
				};
				Action redo = () =>
				{
					var formatting = Root.Song.Formatting;
					formatting.MainText.Size = this.FontSize;
					Root.Song.Formatting = formatting;
				};

				var ch = new DelegateChange(this, undo, redo, new ChangeKey<object, string>(this, "MainTextSize"));
				UndoService.Current[Root].AddChange(ch, "ChangeMainTextSize");

				redo();
			}
		}

		public int BackgroundIndex
		{
			get
			{
				return slide.BackgroundIndex;
			}
			set
			{
				DefaultChangeFactory.OnChanging(this, "BackgroundIndex", slide.BackgroundIndex, value);
				slide.BackgroundIndex = value;

				// need to notify even if the index stays the same!
				OnNotifyPropertyChanged("BackgroundIndex");
				OnNotifyPropertyChanged("Background");
			}
		}

		public SongBackground Background
		{
			get
			{
				return Root.Song.Backgrounds[BackgroundIndex];
			}
		}

		public SongNodeSlide(SongNodeRoot root, SongSlide slide) : base(root)
		{
			this.Title = WordsLive.Resources.Resource.eGridSlideTitle;
			this.slide = slide;
		}

		public SongNodeSlide(SongNodeRoot root) : base(root)
		{
			this.Title = WordsLive.Resources.Resource.eGridSlideTitle;
			this.slide = new SongSlide(root.Song);
			this.FontSize = Root.Song.Formatting.MainText.Size;
		}

		public void SetBackground(SongBackground bg)
		{
			using (new UndoBatch(this, "SetBackground", false))
			{
				int index = Root.AddBackground(bg);
				this.BackgroundIndex = index;
				Root.CleanBackgrounds();
			}
		}

		public SongNodeSlide Copy()
		{
			var s = new SongNodeSlide(Root);
			s.ChangeFontSize(FontSize);
			s.BackgroundIndex = BackgroundIndex;
			s.Text = Text;
			s.Translation = Translation;
			return s;
		}
	}
}
