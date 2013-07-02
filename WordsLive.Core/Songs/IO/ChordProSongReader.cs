/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using System.IO;

namespace WordsLive.Core.Songs.IO
{
	public class ChordProSongReader : ISongReader
	{
		public void Read(Song song, Stream stream)
		{
			using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default, true))
			{
				song.LoadTemplate();

				song.Order.Clear();
				song.Parts.Clear();

				string line;
				bool inTab = false;
				SongPart chorusPart = null;
				string currentText = null;
				string nextPartName = null;
				string currentPartName = null;

				while ((line = reader.ReadLine()) != null)
				{
					var trimmed = line.Trim();
					if (trimmed.StartsWith("#"))
					{
						continue; // ignore comment line
					}

					if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
					{
						var tag = trimmed.Substring(1, trimmed.Length - 2);
						if (tag.StartsWith("title:") || tag.StartsWith("t:"))
						{
							song.Title = tag.Substring(tag.IndexOf(':') + 1);
							nextPartName = null;
							continue;
						}
						else if (tag.StartsWith("subtitle:") || tag.StartsWith("st:"))
						{
							song.Copyright = tag.Substring(tag.IndexOf(':') + 1);
							nextPartName = null;
							continue;
						}
						else if (tag.StartsWith("comment:") || tag.StartsWith("c:") ||
							tag.StartsWith("comment_italic:") || tag.StartsWith("ci:") ||
							tag.StartsWith("comment_box:") || tag.StartsWith("cb:"))
						{
							if (tag.EndsWith(":") && chorusPart == null)
							{
								// we found a comment that might be a part name and we're not in the chorus
								// -> remember it for later use
								var name = tag.Substring(tag.IndexOf(':') + 1);
								nextPartName = name.Substring(0, name.Length - 1);
							}
							continue;
						}
						else if (tag.StartsWith("start_of_tab") || tag.StartsWith("sot"))
						{
							inTab = true;
							nextPartName = null;
							continue;
						}
						else if (tag.StartsWith("end_of_tab") || tag.StartsWith("eot"))
						{
							inTab = false;
							nextPartName = null;
							continue;
						}
						else if (tag.StartsWith("start_of_chorus") || tag.StartsWith("soc"))
						{
							var chorusName = "Chorus";
							if (song.FindPartByName(chorusName) != null)
							{
								int i = 2;
								while (song.FindPartByName(chorusName + " " + i.ToString()) != null)
								{
									i++;
								}
								chorusName = chorusName + " " + i.ToString();
							}

							chorusPart = new SongPart(song, chorusName);
							nextPartName = null;
							continue;
						}
						else if (tag.StartsWith("end_of_chorus") || tag.StartsWith("eoc"))
						{
							if (chorusPart != null)
							{
								// commit slide and part
								if (currentText != null)
								{
									chorusPart.AddSlide(new SongSlide(song) { Text = currentText });
									currentText = null;
								}

								song.AddPart(chorusPart);
								chorusPart = null;
							}
							nextPartName = null;
							continue;
						}
						else if (tag.StartsWith("define"))
						{
							// ignore
							nextPartName = null;
							continue;
						}

						// else accept {...} as normal text
					}

					if (!inTab)
					{
						if (trimmed == String.Empty)
						{
							nextPartName = null;

							if (currentText != null)
							{
								if (chorusPart != null) // in chorus
								{
									// commit slide
									chorusPart.AddSlide(new SongSlide(song) { Text = currentText });
									currentText = null;
								}
								else
								{
									// commit part
									var partName = currentPartName == null ? FindUnusedPartName(song) : currentPartName;
									var part = new SongPart(song, partName);
									part.AddSlide(new SongSlide(song) { Text = currentText });
									song.AddPart(part);
									currentText = null;
								}
							}
						}
						else
						{
							// actual text/chord line -> add to current text
							// need no further parsing because chords are already in correct format (square brackets)
							if (currentText == null)
							{
								currentText = trimmed;

								// use previously remembered part name for this part
								currentPartName = nextPartName;
								nextPartName = null;
							}
							else
							{
								currentText += "\n" + trimmed;
							}
						}
					}
				}

				// TODO: get rid of code duplication
				if (currentText != null)
				{
					if (chorusPart != null) // in chorus
					{
						// commit slide and part
						chorusPart.AddSlide(new SongSlide(song) { Text = currentText });
						currentText = null;
						song.AddPart(chorusPart);
					}
					else
					{
						// commit part
						var partName = currentPartName == null ? FindUnusedPartName(song) : currentPartName;
						var part = new SongPart(song, partName);
						part.AddSlide(new SongSlide(song) { Text = currentText });
						song.AddPart(part);
						currentText = null;
					}
				}
			}

			// add each part to order
			foreach (SongPart part in song.Parts)
			{
				song.Order.Add(new SongPartReference(part));
			}
		}

		/// <summary>
		/// Helper function to generate an unused part name.
		/// </summary>
		/// <param name="song">The song to generate the name for.</param>
		/// <returns>A part name that is guaranteed to be unused in that song.</returns>
		private static string FindUnusedPartName(Song song)
		{
			int i = 1;
			while (song.FindPartByName(i.ToString()) != null)
			{
				i++;
			}
			return i.ToString();
		}
	}
}
