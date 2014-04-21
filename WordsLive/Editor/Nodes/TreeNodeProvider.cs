/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using WordsLive.Core.Songs;

namespace WordsLive.Editor.Nodes
{
	/// <summary>
	/// Provides the child nodes for a song, including the metadata nodes.
	/// This is used in the song structure tree.
	/// </summary>
	public class TreeNodeProvider : INotifyPropertyChanged
	{
		private Song song;

		/// <summary>
		/// Gets or sets the song that this instance uses.
		/// </summary>
		public Song Song
		{
			get
			{
				return song;
			}
			set
			{
				if (song != null)
				{
					song.Parts.CollectionChanged -= songParts_CollectionChanged;
				}

				song = value;

				if (song != null)
				{
					song.Parts.CollectionChanged += songParts_CollectionChanged; 
				}
				
				OnPropertyChanged("Song");
				OnPropertyChanged("Root");
			}
		}

		void songParts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged("Nodes");
		}

		/// <summary>
		/// Gets the root level tree nodes. This is just the song.
		/// </summary>
		public IEnumerable<Song> Root
		{
			get
			{
				yield return Song;
			}
		}

		/// <summary>
		/// Gets the tree nodes. This includes the song parts and the metadata nodes.
		/// </summary>
		public IEnumerable<ISongElement> Nodes
		{
			get
			{
				foreach (var part in Song.Parts)
				{
					yield return part;
				}

				yield return new SourceNode(Song);
				yield return new CopyrightNode(Song);
				yield return new LanguageNode(Song);
				yield return new CategoryNode(Song);
				yield return new CcliNumberNode(Song);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
