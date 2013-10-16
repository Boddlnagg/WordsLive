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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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

		public static readonly RoutedEvent NavigateBeyondFirstEvent =
		EventManager.RegisterRoutedEvent(
			"NavigateBeyondFirst",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(SongSlideListBox));

		public static readonly RoutedEvent NavigateBeyondLastEvent =
		EventManager.RegisterRoutedEvent(
			"NavigateBeyondLast",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(SongSlideListBox));

		public event RoutedEventHandler NavigateBeyondFirst
		{
			add { AddHandler(NavigateBeyondFirstEvent, value); }
			remove { RemoveHandler(NavigateBeyondFirstEvent, value); }
		}

		public event RoutedEventHandler NavigateBeyondLast
		{
			add { AddHandler(NavigateBeyondLastEvent, value); }
			remove { RemoveHandler(NavigateBeyondLastEvent, value); }
		}

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

		protected override void OnKeyDown(KeyEventArgs e)
		{
			int previousIndex = SelectedIndex;

			if (e.Key == Key.Enter || e.Key == Key.Return)
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
			else if (e.Key == Key.PageDown || e.Key == Key.Down || e.Key == Key.Right || e.Key == Key.Space)
			{
				if (SelectedIndex + 1 < Items.Count)
				{
					SelectedIndex++;
					e.Handled = true;
				}
				else
				{
					RaiseEvent(new RoutedEventArgs(NavigateBeyondLastEvent, this));
				}
			}
			else if (e.Key == Key.PageUp || e.Key == Key.Up || e.Key == Key.Left)
			{
				if (SelectedIndex > 0)
				{
					SelectedIndex--;
					e.Handled = true;
				}
				else
				{
					RaiseEvent(new RoutedEventArgs(NavigateBeyondFirstEvent, this));
				}
			}
			else
			{
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
			}

			ScrollBeyond(previousIndex, SelectedIndex);

			base.OnKeyUp(e);
		}

		private void ScrollBeyond(int oldIndex, int newIndex)
		{
			if (newIndex > oldIndex && newIndex + 1 < Items.Count)
			{
				// scroll down one more slide
				ScrollIntoView(Items[newIndex + 1]);
			}
			else if (newIndex < oldIndex && newIndex > 0)
			{
				// scroll up one more slide
				ScrollIntoView(Items[newIndex - 1]);
			}
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