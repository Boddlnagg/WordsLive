using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using WordsLive.Core.Songs;
using WordsLive.Utils;

namespace WordsLive.Songs
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

			foreach (var partRef in song.Order)
			{
				string accessKey;
				int accessIndex;

				if (!partAccessIndices.ContainsKey(partRef.Part.Name))
				{
					var match = Regex.Match(partRef.Part.Name, @"^[^\d]*(\d)[^\d]*$");
					if (match != Match.Empty)
					{
						var keyIndex = match.Groups[1].Index + match.Groups[1].Length - 1;
						accessIndex = keyIndex;
						accessKey = partRef.Part.Name.Substring(keyIndex, 1).ToUpper();
					}
					else
					{
						// use first letter
						accessIndex = 0;
						accessKey = partRef.Part.Name[0].ToString().ToUpper();
					}

					if (!partAccessKeys.ContainsKey(accessKey))
					{
						partAccessKeys.Add(accessKey, partRef.Part.Name);
						partAccessIndices.Add(partRef.Part.Name, accessIndex);
					}
					else
					{
						partAccessIndices.Add(partRef.Part.Name, -1); // no access key possible
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
			foreach (var partRef in song.Order)
			{
				int slideIndex = 0;
				foreach (var s in (from p in this.song.Parts where p.Name == partRef.Part.Name select p).Single().Slides)
				{
					var accIndex = partAccessIndices[partRef.Part.Name];
					string pre;
					string acc;
					string post;
					if (accIndex >= 0)
					{
						pre = partRef.Part.Name.Substring(0, partAccessIndices[partRef.Part.Name]);
						acc = partRef.Part.Name[partAccessIndices[partRef.Part.Name]].ToString();
						post = partRef.Part.Name.Substring(partAccessIndices[partRef.Part.Name] + 1);
					}
					else
					{
						pre = partRef.Part.Name;
						acc = "";
						post = "";
					}

					parts.Add(new SongSlideContainer
					{
						Text = s.TextWithoutChords,
						Translation = s.Translation,
						Background = song.Backgrounds[s.BackgroundIndex],
						PartIndex = partIndex,
						PartName = partRef.Part.Name,
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

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
			{
				if (Keyboard.FocusedElement is ListBoxItem)
				{
					var item = Keyboard.FocusedElement as ListBoxItem;
					if (item.FindVisualParent<SongSlideListBox>() == this)
					{
						SelectedItem = item.Content;
						e.Handled = true;
						return;
					}
				}
			}

			string part;
			if (partAccessKeys.ContainsKey(e.Key.ToString()))
				part = partAccessKeys[e.Key.ToString()];
			else if (partAccessKeys.ContainsKey(e.Key.ToString().Replace("NumPad", "")))
				part = partAccessKeys[e.Key.ToString().Replace("NumPad", "")];
			else if (e.Key.ToString().Length == 2 && e.Key.ToString()[0] == 'D' && partAccessKeys.ContainsKey(e.Key.ToString().Substring(1)))
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