using System.Collections.ObjectModel;
using System.Linq;
using MonitoredUndo;
using WordsLive.Core.Songs;
using System;

namespace WordsLive.Editor
{
	public class SongNodePart : SongNode
	{
		private ObservableCollection<SongNodeSlide> slides = new ObservableCollection<SongNodeSlide>();
		private SongPart part;
		private Song song;

		public override string IconUri
		{
			get
			{
				return "/WordsLive;component/Artwork/Small_Folder.png";
			}
		}

		public SongPart Part
		{
			get
			{
				return part;
			}
		}

		public SongNodePart(SongNodeRoot root, Song song, SongPart part) : base(root)
		{
			this.part = part;
			this.song = song;

			foreach (SongSlide slide in part.Slides)
			{
				slides.Add(new SongNodeSlide(Root, slide));
			}

			this.Title = part.Name;

			this.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Title")
				{
					if (this.part.Name != this.Title)
					{
						int i;
						while ((i = song.Order.IndexOf(this.part.Name)) >= 0)
						{
							song.Order[i] = this.Title;
						}

						this.part.Name = this.Title;
					}
				}
			};

			slides.CollectionChanged += (sender, args) =>
			{
				part.Slides = (from slide in slides select slide.Slide).ToList();
				//DefaultChangeFactory.OnCollectionChanged(this, "Children", this.Children, args); // this does not work correctly
			};
		}

		public SongNodePart(SongNodeRoot root, Song song, string name) : this(root, song, new SongPart {Name = name})
		{
			this.AddSlide();
		}

		public SongNodeSlide AddSlide()
		{
			var slide = new SongNodeSlide(Root);
			AddSlide(slide);
			return slide;
		}

		public void AddSlide(SongNodeSlide slide)
		{
			var ch = new DelegateChange(this,
				() => { slides.Remove(slide); },
				() => { slides.Add(slide); },
				new ChangeKey<object, string>(this, "Children"));
			UndoService.Current[Root].AddChange(ch, "AddSlide");
			this.slides.Add(slide);
		}

		public void InsertSlideAfter(SongNodeSlide slide, SongNodeSlide target)
		{
			var index = this.slides.IndexOf(target);
			if (index < 0)
				throw new InvalidOperationException("Slide is not in this part.");

			if (index >= this.slides.Count - 1)
			{
				AddSlide(slide);
			}
			else
			{
				var ch = new DelegateChange(this,
				() => { slides.Remove(slide); },
				() => { slides.Insert(index + 1, slide); },
				new ChangeKey<object, string>(this, "Children"));
				UndoService.Current[Root].AddChange(ch, "InsertSlideAfter");
				this.slides.Insert(index + 1, slide);
			}
		}

		public void RemoveSlide(SongNodeSlide slide)
		{
			if (this.slides.Count <= 1)
				throw new InvalidOperationException("Can't remove last slide in a part.");

			int i = slides.IndexOf(slide);
			if (i < 0)
				throw new InvalidOperationException("Slide is not in this part.");

			var ch = new DelegateChange(this,
				() => { slides.Insert(i, slide); },
				() => { slides.Remove(slide); },
				new ChangeKey<object, string>(this, "Children"));
			UndoService.Current[Root].AddChange(ch, "RemoveSlide");
			this.slides.Remove(slide);
		}

		public SongNodeSlide DuplicateSlide(SongNodeSlide slide)
		{
			SongNodeSlide s;
			int i = slides.IndexOf(slide);
			using (new UndoBatch(this, "DuplicateSlide", false))
			{
				s = slide.Copy();
				var ch = new DelegateChange(this,
					() => { slides.Remove(s); },
					() => { slides.Insert(i + 1, s); },
					new ChangeKey<object, string>(this, "Children"));
				UndoService.Current[Root].AddChange(ch, "DuplicateSlide");
				slides.Insert(i + 1, s);
			}
			return s;
		}

		public SongNodeSlide SplitSlide(SongNodeSlide slide, int splitIndex)
		{
			SongNodeSlide newSlide;
			using (new UndoBatch(this, "SplitSlide", false))
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

		public ObservableCollection<SongNodeSlide> Children
		{
			get
			{
				return slides;
			}
		}

		/// <summary>
		/// Sets the background for every slide in the part.
		/// </summary>
		/// <param name="bg"></param>
		public void SetBackground(SongBackground bg)
		{
			using (new UndoBatch(this, "SetBackground", false))
			{
				int index = Root.AddBackground(bg);

				foreach (var slide in this.Children)
				{
					slide.BackgroundIndex = index;
				}

				Root.CleanBackgrounds();
			}
		}

		public SongNodePart Copy(string name)
		{
			var copy = new SongNodePart(Root, song, name);
			copy.slides.Clear();
			foreach(var slide in slides)
			{
				copy.AddSlide(slide.Copy());
			}
			return copy;
		}
	}
}
