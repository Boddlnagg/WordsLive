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

namespace WordsLive.Core.Songs.Storage
{
	public class SongUriResolver
	{
		public SongStorage ForceStorage { get; private set; }

		public SongUriResolver() : this (null) { }

		public SongUriResolver(SongStorage forceStorage)
		{
			ForceStorage = forceStorage;
		}

		private static SongUriResolver defaultInstance;

		public static SongUriResolver Default
		{
			get
			{
				if (defaultInstance == null)
					defaultInstance = new SongUriResolver();

				return defaultInstance;
			}
		}

		//public static Song Load(Uri uri)
		//{
			
		//}

		//public static void Save(Song song, Uri uri)
		//{
			
		//}

		public Stream Get(Uri uri)
		{
			if (uri.Scheme == "song")
			{
				return (ForceStorage ?? DataManager.Songs).Get(GetFilename(uri));
				
			}
			else if (uri.IsFile)
			{
				try
				{
					return File.OpenRead(uri.LocalPath);
				}
				catch (DirectoryNotFoundException)
				{
					throw new FileNotFoundException(uri.LocalPath);
				}
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public FileTransaction Put(Uri uri)
		{
			if (uri.Scheme == "song")
			{
				return (ForceStorage ?? DataManager.Songs).Put(GetFilename(uri));
			}
			else if (uri.IsFile)
			{
				return new LocalFileTransaction(uri.LocalPath);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		private static string GetFilename(Uri uri)
		{
			if (uri.Scheme != "song")
				throw new ArgumentException("uri");

			return Uri.UnescapeDataString(uri.AbsolutePath).Substring(1);
		}
	}
}
