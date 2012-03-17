using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Words.Core.Songs;

namespace Words.Songs
{
	public partial class SongSlideListBox : ListBox
	{
		private CollectionViewSource cvs;
		private Song song;

		public SongSlide SelectedSlide
		{
			get
			{
				SongSlideContainer container = (this.SelectedItem as SongSlideContainer);
				return (from p in this.song.Parts where p.Name == container.PartName select p).Single().Slides[container.SlideIndex];
			}
		}

		public SongSlideListBox()
		{
			this.InitializeComponent();

			cvs = new CollectionViewSource();
			cvs.GroupDescriptions.Add(new PropertyGroupDescription("PartIndex"));
			this.DataContext = cvs;
		}

		public Song Song
		{
			get
			{
				return song;
			}
			set
			{
				song = value;
				cvs.Source = GenerateContainers();
			}
		}

		private List<SongSlideContainer> GenerateContainers()
		{
			List<SongSlideContainer> parts = new List<SongSlideContainer>();
			int partIndex = 0;
			int orderPosition = 0;
			parts.Add(new SongSlideContainer
			{
				Text = "(Nur Hintergrund)",
				Translation = "",
				Background = SongBackgroundToImageSourceConverter.CreateBackgroundSource(song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0]),
				PartIndex = partIndex++,
				PartName = "",
				SlideIndex = 0,
				OrderPosition = orderPosition++
			});
			foreach (var partName in song.Order)
			{
				int slideIndex = 0;
				foreach (var s in (from p in this.song.Parts where p.Name == partName select p).Single().Slides)
				{
					parts.Add(new SongSlideContainer
					{
						Text = s.TextWithoutChords,
						Translation = s.Translation,
						Background = SongBackgroundToImageSourceConverter.CreateBackgroundSource(song.Backgrounds[s.BackgroundIndex]),
						PartIndex = partIndex,
						PartName = partName,
						SlideIndex = slideIndex++,
						OrderPosition = orderPosition++
					});
				}
				partIndex++;
			}

			parts.Add(new SongSlideContainer
			{
				Text = "(Nur Hintergrund)",
				Translation = "",
				Background = SongBackgroundToImageSourceConverter.CreateBackgroundSource(song.Backgrounds[song.LastSlide != null ? song.LastSlide.BackgroundIndex : 0]),
				PartIndex = partIndex++,
				PartName = "",
				SlideIndex = 0,
				OrderPosition = orderPosition++
			});
			return parts;
		}

		public class SongSlideContainer
		{
			public string Text { get; set; }
			public string Translation { get; set; }
			public ImageSource Background { get; set; }
			public int PartIndex { get; set; }
			public string PartName { get; set; }
			public int SlideIndex { get; set; }
			public int OrderPosition { get; set; } // this is needed so every slide will be unique
		}
	}
}