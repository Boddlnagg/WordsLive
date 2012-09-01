using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
		private Dictionary<string, string> partAccessKeys = new Dictionary<string, string>();
		private Dictionary<string, int> partAccessIndices = new Dictionary<string, int>();

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
			partAccessIndices.Clear();

			foreach (var partName in song.Order)
			{
				string accessKey;
				int accessIndex;

				if (!partAccessIndices.ContainsKey(partName))
				{
					var match = Regex.Match(partName, @"^[^\d]*(\d)[^\d]*$");
					if (match != Match.Empty)
					{
						var keyIndex = match.Groups[1].Index + match.Groups[1].Length - 1;
						accessIndex = keyIndex;
						accessKey = partName.Substring(keyIndex, 1).ToUpper();
					}
					else
					{
						// use first letter
						accessIndex = 0;
						accessKey = partName[0].ToString().ToUpper();
					}

					if (!partAccessKeys.ContainsKey(accessKey))
					{
						partAccessKeys.Add(accessKey, partName);
						partAccessIndices.Add(partName, accessIndex);
					}
					else
					{
						partAccessIndices.Add(partName, -1); // no access key possible
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
				Background = song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0],
				PartIndex = partIndex++,
				PartName = "",
				PreAccessName = "",
				AccessKey = "",
				PostAccessName = "",
				SlideIndex = 0,
				OrderPosition = orderPosition++
			});
			foreach (var partName in song.Order)
			{
				int slideIndex = 0;
				foreach (var s in (from p in this.song.Parts where p.Name == partName select p).Single().Slides)
				{
					var accIndex = partAccessIndices[partName];
					string pre;
					string acc;
					string post;
					if (accIndex >= 0)
					{
						pre = partName.Substring(0, partAccessIndices[partName]);
						acc = partName[partAccessIndices[partName]].ToString();
						post = partName.Substring(partAccessIndices[partName] + 1);
					}
					else
					{
						pre = partName;
						acc = "";
						post = "";
					}

					parts.Add(new SongSlideContainer
					{
						Text = s.TextWithoutChords,
						Translation = s.Translation,
						Background = song.Backgrounds[s.BackgroundIndex],
						PartIndex = partIndex,
						PartName = partName,
						PreAccessName = pre,
						AccessKey = acc,
						PostAccessName = post,
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
				Background = song.Backgrounds[song.LastSlide != null ? song.LastSlide.BackgroundIndex : 0],
				PartIndex = partIndex++,
				PartName = "",
				PreAccessName = "",
				AccessKey = "",
				PostAccessName = "",
				SlideIndex = 0,
				OrderPosition = orderPosition++
			});
			return parts;
		}

		public class SongSlideContainer
		{
			public string Text { get; set; }
			public string Translation { get; set; }
			public SongBackground Background { get; set; }
			public int PartIndex { get; set; }
			public string PartName { get; set; }
			public string PreAccessName { get; set; }
			public string AccessKey { get; set; }
			public string PostAccessName { get; set; }
			public int SlideIndex { get; set; }
			public int OrderPosition { get; set; } // this is needed so every slide will be unique
		}

		protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
		{
			string part;
			if (partAccessKeys.ContainsKey(e.Key.ToString()))
				part = partAccessKeys[e.Key.ToString()];
			else if (partAccessKeys.ContainsKey(e.Key.ToString().Replace("NumPad", "")))
				part = partAccessKeys[e.Key.ToString().Replace("NumPad", "")];
			else if (e.Key.ToString().Length > 1 && partAccessKeys.ContainsKey(e.Key.ToString().Substring(1)))
				part = partAccessKeys[e.Key.ToString().Substring(1)];
			else
				return;

			e.Handled = true;

			var selectedContainer = FindNextItemWithPartName(part);

			this.SelectedItem = selectedContainer;

			base.OnKeyUp(e);
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			var container = (ListBoxItem)ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
			if (container != null)
				container.Focus();

			base.OnSelectionChanged(e);
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
	}
}