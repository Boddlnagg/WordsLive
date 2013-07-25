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

using System.ComponentModel;
using WordsLive.Core.Songs;

namespace WordsLive.Editor.Nodes
{
	/// <summary>
	/// Represents the node for the copyright metadata.
	/// </summary>
	public class CopyrightNode : MetadataNode, ISongElementWithSize, INotifyPropertyChanged
	{
		public int Size
		{
			get
			{
				return Root.Formatting.CopyrightText.Size;
			}
			set
			{
				var formatting = (SongFormatting)Root.Formatting.Clone();
				formatting.CopyrightText.Size = value;
				Root.Formatting = formatting;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyrightNode"/> class.
		/// </summary>
		/// <param name="song">The song.</param>
		public CopyrightNode(Song song) : base(song)
		{
			song.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Formatting")
					OnPropertyChanged("Size");
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
