using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Words.Core.Songs;
using System.Windows;
using System.Text.RegularExpressions;
using System;

namespace Words.Songs
{
	public partial class SongSlideListBox : ListBox
	{
		private CollectionViewSource cvs;
		private Song song;
		private List<string> partAccessKeys = new List<string>();
		private Dictionary<string, string> partAccessNames = new Dictionary<string, string>();

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
				GeneratePartAccessKeys();
				cvs.Source = GenerateContainers();
			}
		}

		private void GeneratePartAccessKeys()
		{
			partAccessKeys.Clear();
			partAccessNames.Clear();

			foreach (var partName in song.Order)
			{
				string accessKey;
				string accessName;

				if (!partAccessNames.ContainsKey(partName))
				{
					var match = Regex.Match(partName, @"^[^\d]*(\d)[^\d]*$");
					if (match != Match.Empty)
					{
						var keyIndex = match.Groups[1].Index + match.Groups[1].Length - 1;
						accessName = partName.Insert(keyIndex, "_");
						accessKey = partName.Substring(keyIndex, 1);
					}
					else
					{
						// use first letter
						accessName = "_" + partName;
						accessKey = partName[0].ToString();
					}

					if (!partAccessKeys.Contains(accessKey))
					{
						partAccessKeys.Add(accessKey);
						partAccessNames.Add(partName, accessName);
					}
					else
					{
						partAccessNames.Add(partName, partName); // no access key possible
					}
				}
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
				AccessName = "",
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
						AccessName = partAccessNames[partName],
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
				AccessName = "",
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
			public string AccessName { get; set; }
			public int SlideIndex { get; set; }
			public int OrderPosition { get; set; } // this is needed so every slide will be unique
		}

		private bool sameKeyEvent;
		private bool? lastKeyEvent;

		private void Part_AccessKeyPressed(object sender, System.Windows.Input.AccessKeyPressedEventArgs e)
		{
			if (!partAccessKeys.Contains(e.Key))
				return;

			e.Handled = true;

			if (lastKeyEvent.HasValue && lastKeyEvent.Value == sameKeyEvent)
				return;

			lastKeyEvent = sameKeyEvent;

			var cvg = (sender as FrameworkElement).DataContext as CollectionViewGroup;

			if (cvg == null)
				return;

			var selectedPartName = (cvg.Items[0] as SongSlideContainer).PartName;

			var selectedContainer = FindNextItemWithPartName(selectedPartName);

			this.SelectedItem = selectedContainer;
			this.ScrollIntoView(selectedContainer);
		}

		private SongSlideContainer FindNextItemWithPartName(string name)
		{
			for (int i = this.SelectedIndex + 1; i < this.Items.Count; i++)
			{
				if ((this.Items[i] as SongSlideContainer).PartName == name)
					return (this.Items[i] as SongSlideContainer);
			}

			for (int i = 0; i <= this.SelectedIndex; i++)
			{
				if ((this.Items[i] as SongSlideContainer).PartName == name)
					return (this.Items[i] as SongSlideContainer);
			}

			throw new ArgumentException("Part with name \"" + name + "\" does not exist");
		}

		protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			sameKeyEvent = !sameKeyEvent;
			lastKeyEvent = null;

			base.OnPreviewKeyDown(e);
		}
	}
}