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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Ionic.Zip;
using WordsLive.Core;

namespace WordsLive.Images
{
	public class ImagesMedia : Media
	{
		public static readonly string[] ImageExtensions = new string[] { ".jpg", ".jpeg", ".png" }; // TODO: add more
		public static readonly string[] SlideshowExtensions = new string[] { ".show", ".zip" };

		public ObservableCollection<ImageInfo> Images { get; private set; }

		public bool CanSave { get; private set; }

		public bool CanEdit { get; private set; }

		public ImagesMedia(Uri uri) : base(uri) { }

		protected override void LoadMetadata()
		{
			base.LoadMetadata();
			Load(); // load slideshow stats if it is one -> able to show number of images
		}

		public override void Load()
		{
			string ext = Uri.GetExtension().ToLower();
			if (ext == ".show")
			{
				Images = new ObservableCollection<ImageInfo>(LoadFromTxt());
				CanSave = true;
				CanEdit = true;
			}
			else if (ext == ".zip")
			{
				Images = new ObservableCollection<ImageInfo>(LoadFromZip());
				CanSave = false;
				CanEdit = false;
			}
			else
			{
				Images = new ObservableCollection<ImageInfo> { new ImageInfo(Uri) };
				CanSave = false;
				CanEdit = true;
			}

			if (!Uri.IsFile)
				CanSave = false;
		}

		internal void CreateSlideshow(IEnumerable<Uri> uris)
		{
			Images = new ObservableCollection<ImageInfo>();
			InsertImages(uris, 0);
			CanSave = true;
			CanEdit = true;
			Save();
		}

		private IEnumerable<ImageInfo> LoadFromTxt()
		{
			if (!Uri.IsFile)
				throw new NotImplementedException("Loading slideshows from a remote source is not implemented.");

			using (var reader = new StreamReader(Uri.LocalPath))
			{
				string line = reader.ReadLine();

				if (line == null)
					yield break;

				Uri alternateBaseUri = null;

				if (line.StartsWith("#"))
				{
					alternateBaseUri = new Uri(line.Substring(1).Trim());
					line = reader.ReadLine(); // read next line
				}

				while (line != null)
				{
					ImageInfo next = null;
					try
					{
						var relativeUri = new Uri(line, UriKind.RelativeOrAbsolute);
						if (relativeUri.IsAbsoluteUri)
							next = new ImageInfo(relativeUri);
						else
						{
							var uri = new Uri(Uri, relativeUri);
							if (uri.IsFile && !File.Exists(uri.LocalPath))
							{
								// try alternate base uri if the file does not exist
								uri = new Uri(alternateBaseUri, relativeUri);
							}
							
							next = new ImageInfo(uri);
						}
					}
					catch (UriFormatException)
					{
						continue;
					}
					finally
					{
						line = reader.ReadLine(); // read next line
					}

					yield return next;
				}
			}
		}

		private IEnumerable<ImageInfo> LoadFromZip()
		{
			if (!Uri.IsFile)
				throw new NotImplementedException("Loading slideshows from a remote source is not implemented.");

			Stream stream = System.IO.File.OpenRead(Uri.LocalPath);
			using (var zip = ZipFile.Read(stream))
			{
				foreach (var entry in zip.Entries)
				{
					yield return new ImageInfo(entry);
				}
			}

			// important: don't close stream directly, so ImageLoader can load the images
			// TODO: close & dispose the stream when it isn't needed anymore
		}

		public void InsertImages(IEnumerable<Uri> uris, int index)
		{
			int i = index;
			foreach (var uri in uris)
			{
				if (IsValidImageUri(uri))
				{
					Images.Insert(i++, new ImageInfo(uri));
				}
			}
		}

		public void Save()
		{
			if (!CanSave || !Uri.IsFile)
				throw new InvalidOperationException("Cannot save this ImagesMedia.");

			using (var writer = new StreamWriter(Uri.LocalPath))
			{
				// write the full path of the slideshow file itself in the first line
				// in order to fall back to absolute paths in case the slideshow was moved
				// and the images were not
				writer.WriteLine("#" + Uri.LocalPath);
				foreach (var img in Images)
				{
					if (img.Uri.IsFile)
					{
						writer.WriteLine(Uri.MakeRelativeUri(img.Uri).FormatLocal());
					}
					else
					{
						writer.WriteLine(img.Uri.AbsoluteUri);
					}
				}
			}
		}

		public static bool IsValidImageUri(Uri uri)
		{
			var ext = uri.GetExtension().ToLower();
			return ImageExtensions.Contains(ext);
		}
	}
}
