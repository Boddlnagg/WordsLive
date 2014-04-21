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

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// Importer for CCLI SongSelect files in .usr format
	/// </summary>
	public class CcliUsrSongReader : CcliSongReader
	{
		// documentation taken from http://bazaar.launchpad.net/~openlp-core/openlp/trunk/view/head:/openlp/plugins/songs/lib/cclifileimport.py

		// [File]  
		// USR file format first line
   
		// Type=  
		// Indicates the file type
		// e.g. Type=SongSelect Import File
   
		// Version=3.0  
		// File format version
   
		// [S A2672885]
		// Contains the CCLI Song number e.g. 2672885
   
		// Title=
		// Contains the song title (e.g. Title=Above All)
   
		// Author=  
		// Contains a | delimited list of the song authors
		// e.g. Author=LeBlanc, Lenny | Baloche, Paul
   
		// Copyright=  
		// Contains a | delimited list of the song copyrights
		// e.g. Copyright=1999 Integrity's Hosanna! Music |
		// LenSongs Publishing (Verwaltet von Gerth Medien
		// Musikverlag)
   
		// Admin=  
		// Contains the song administrator
		// e.g. Admin=Gerth Medien Musikverlag
   
		// Themes=  
		// Contains a /t delimited list of the song themes
		// e.g. Themes=Cross/tKingship/tMajesty/tRedeemer
   
		// Keys=  
		// Contains the keys in which the music is played??
		// e.g. Keys=A
   
		// Fields=  
		// Contains a list of the songs fields in order /t delimited
		// e.g. Fields=Vers 1/tVers 2/tChorus 1/tAndere 1
   
		// Words=  
		// Contains the songs various lyrics in order as shown by the
		// Fields description
		// e.g. Words=Above all powers.... [/n = CR, /n/t = CRLF]

		public override void Read(Song song, Stream stream)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (stream == null)
				throw new ArgumentNullException("stream");

			using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
			{
				string line;
				string authors = null;
				string copyright = null;
				string[] fieldsList = null;
				string[] wordsList = null;

				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("[S "))
					{
						// CCLI Song number
						int end = line.IndexOf(']');
						if (end > 1)
						{
							string num = line.Substring(3, end - 3);
							if (num.StartsWith("A"))
								num = num.Substring(1);

							song.CcliNumber = int.Parse(num);
						}
					}
					else if (line.StartsWith("Title="))
					{
						song.Title = line.Substring("Title=".Length).Trim();
					}
					else if (line.StartsWith("Author="))
					{
						var authorList = line.Substring("Author=".Length).Trim().Split('|').Select(s => s.Trim()).ToArray();
						authors = String.Join(", ", authorList);
					}
					else if (line.StartsWith("Copyright="))
					{
						copyright = line.Substring("Copyright=".Length).Trim();
					}
					else if (line.StartsWith("Themes="))
					{
						var themesList = line.Substring("Themes=".Length).Trim().Replace(" | ", "/t").
							Split(new string[] { "/t" }, StringSplitOptions.None).Select(s => s.Trim()).ToArray();
						 song.Category = String.Join(", ", themesList);
					}
					else if (line.StartsWith("Fields="))
					{
						fieldsList = line.Substring("Fields=".Length).Trim().Split(new string[] {"/t"}, StringSplitOptions.None).Select(s => s.Trim()).ToArray();
					}
					else if (line.StartsWith("Words="))
					{
						wordsList = line.Substring("Words=".Length).Trim().Split(new string[] { "/t" }, StringSplitOptions.None).Select(s => s.Trim()).ToArray();
					}

					//	Unhandled usr keywords: Type, Version, Admin, Keys
				}

				if (fieldsList == null || wordsList == null || authors == null || copyright == null)
				{
					throw new SongFormatException("Missing field in USR file.");
				}

				var partNum = (fieldsList.Length < wordsList.Length) ? fieldsList.Length : wordsList.Length;

				for (int i = 0; i < partNum; i++)
				{
					bool checkFirstLine;
					var partName = GetPartName(fieldsList[i], out checkFirstLine);

					string text = wordsList[i].Replace("/n", "\n").Replace(" | ", "\n").TrimEnd();

					if (checkFirstLine)
					{
						var lines = text.Split('\n');
						var firstLine = lines[0].Trim();
						if (CheckFirstLine(firstLine, ref partName))
						{
							text = text.Substring(text.IndexOf('\n') + 1);
						}
					}

					var part = new SongPart(song, partName);
					var slide = new SongSlide(song);
					slide.Text = text;
					part.AddSlide(slide);
					song.AddPart(part);
					song.AddPartToOrder(part);
				}

				song.Copyright = authors + "\n© " + copyright;
			}
		}
	}
}
