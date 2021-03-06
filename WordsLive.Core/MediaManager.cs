﻿/*
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
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WordsLive.Core
{
	/// <summary>
	/// Manages the available media types and file handlers.
	/// </summary>
	public static class MediaManager
	{
		private static List<MediaTypeHandler> handlers = new List<MediaTypeHandler>();

		/// <summary>
		/// Registers file handlers from the given types. Any non-abstract subclass of <see cref="MediaFileHandler"/>
		/// that is found will be registered.
		/// </summary>
		/// <param name="types"></param>
		public static void RegisterHandlersFromTypes(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				if (type.IsClass && !type.IsAbstract && typeof(MediaTypeHandler).IsAssignableFrom(type))
				{
					MediaTypeHandler handler = (MediaTypeHandler)Activator.CreateInstance(type);
					handlers.Add(handler);
				}
			}
		}

		/// <summary>
		/// Gets the registered media type handlers.
		/// </summary>
		public static IEnumerable<MediaTypeHandler> Handlers
		{
			get
			{
				return handlers;
			}
		}

		/// <summary>
		/// Creates a media object from a URI.
		/// If the URI does not reference an HTTP resource, a <see cref="FileNotFoundMedia"/> is returned if
		/// the resource does not exist.
		/// If no appropiate file handler is found, a <see cref="UnsupportedMedia"/> is returned.
		/// Otherwise the correct <see cref="Media"/> type is returned.
		/// </summary>
		/// <param name="uri">The URI to load from. This can either be local (file://), remote (http://)
		/// or a reference to the song database (song://).</param>
		/// <returns>A <see cref="Media"/> object.</returns>
		public static Media LoadMediaMetadata(Uri uri, Dictionary<string, string> options)
		{
			try
			{
				MediaTypeHandler maxPriorityHandler = handlers.First();
				int maxPriority = maxPriorityHandler.Test(uri);

				foreach (var h in handlers.Skip(1))
				{
					int priority = h.Test(uri);
					if (priority > maxPriority)
					{
						maxPriorityHandler = h;
						maxPriority = priority;
					}
				}

				if (maxPriority < 0)
				{
					return new UnsupportedMedia(uri);
				}
				else
				{
					var result = maxPriorityHandler.Handle(uri, options);
					result.LoadMetadataHelper();
					return result;
				}
			}
			catch (FileNotFoundException)
			{
				return new FileNotFoundMedia(uri);
			}
			catch (DirectoryNotFoundException)
			{
				return new FileNotFoundMedia(uri);
			}
		}

		/// <summary>
		/// Tries to loads multiple files at once using the a single handler's <see cref="MediaTypeHandler.HandleMultiple"/> method
		/// in order to load them into a single media object if supported. This assumes that the files exist.
		/// </summary>
		/// <param name="uris">The URIs to load.</param>
		/// <returns>The loaded media objects, either one per file or less.</returns>
		public static IEnumerable<Media> LoadMultipleMediaMetadata(IEnumerable<Uri> uris)
		{
			MediaTypeHandler maxPriorityHandler = handlers.First();
			int maxPriority = maxPriorityHandler.TestMultiple(uris);

			foreach (var h in handlers.Skip(1))
			{
				int priority = h.TestMultiple(uris);
				if (priority > maxPriority)
				{
					maxPriorityHandler = h;
					maxPriority = priority;
				}
			}

			if (maxPriority < 0)
			{
				// not all of them are supported by a single handler => load them separately
				return uris.Select(u => LoadMediaMetadata(u, null));
				
			}
			else
			{
				var result = maxPriorityHandler.HandleMultiple(uris);
				foreach (var m in result)
				{
					m.LoadMetadataHelper();
				}
				return result;
			}
		}

		/// <summary>
		/// Reloads the metadata of a media object.
		/// </summary>
		/// <param name="media">The object to reload.</param>
		/// <returns>The reloaded media object. Possibly but not necessarily the same object that was given as input.</returns>
		public static Media ReloadMediaMetadata(Media media)
		{
			try
			{
				if (media is FileNotFoundMedia || media is UnsupportedMedia)
				{
					return LoadMediaMetadata(media.Uri, media.Options);
				}
				else
				{
					media.LoadMetadataHelper();
					return media;
				}
			}
			catch (FileNotFoundException)
			{
				return new FileNotFoundMedia(media.Uri);
			}
			catch (DirectoryNotFoundException)
			{
				return new FileNotFoundMedia(media.Uri);
			}
		}

		/// <summary>
		/// Loads a given media object by calling it's <see cref="Media.Load"/> method and
		/// triggering the <see cref="MediaLoaded"/> event.
		/// </summary>
		/// <param name="media"></param>
		public static void LoadMedia(Media media)
		{
			// TODO: make async?
			if (media == null)
				throw new ArgumentNullException("media");

			media.Load();
			OnMediaLoaded(media);
		}

		/// <summary>
		/// This event is called when a media object was loaded.
		/// </summary>
		public static event EventHandler<MediaEventArgs> MediaLoaded; // TODO: rename to MediaLoading

		private static void OnMediaLoaded(Media media)
		{
			if (MediaLoaded != null)
				MediaLoaded(null, new MediaEventArgs(media));
		}

		/// <summary>
		/// Tries to load a file as Powerpraise portfolio file.
		/// </summary>
		/// <param name="fileName">The file.</param>
		/// <param name="result">The loaded media objects or <c>null</c> if loading failed.</param>
		/// <returns><c>True</c> if the file was a valid Powerpraise portfolio and has been loaded, <c>false</c> otherwise.</returns>
		public static bool TryLoadPortfolio(string fileName, out IEnumerable<Media> result)
		{
			result = LoadPortfolio(fileName);
			if (result.Count() == 0)
			{
				result = null;
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Loads a Powerpraise portfolio file.
		/// TODO: move to a separate class
		/// </summary>
		/// <param name="fileName">The portfolio file.</param>
		/// <returns>The loaded media objects in the order they appear in the portfolio.</returns>
		public static IEnumerable<Media> LoadPortfolio(string fileName)
		{
			if (File.Exists(fileName))
			{
				FileInfo file = new FileInfo(fileName);
				if (file.Extension == ".ppp")
				{
					XDocument doc = XDocument.Load(file.FullName);
					XElement root = doc.Element("ppp");
					if (root.Attribute("version").Value == "3.0" || root.Attribute("version").Value == "4.0")
					{
						foreach (Media m in from i in root.Element("order").Elements("item")
											select (i.Attribute("mediatype").Value == "powerpraise-song" && !i.Element("file").Value.Contains('\\')) ?
											LoadMediaMetadata(new Uri("song:///" + i.Element("file").Value), LoadOptions(i)) :
											LoadMediaMetadata(new Uri(i.Element("file").Value), LoadOptions(i)))
						{
							yield return m;
						}

						yield break;
					}
					else if (root.Attribute("version").Value == "2.2")
					{
						foreach (Media m in from i in root.Elements("item")
													select MediaManager.LoadMediaMetadata(new Uri("song:///" + i.Element("file").Value), null))
						{
							yield return m;
						}

						yield break;
					}
					else
					{
						throw new ApplicationException("Could not load portfolio.");
					}
				}
			}
		}

		private static Dictionary<string, string> LoadOptions(XElement element)
		{
			var result = new Dictionary<string, string>();
			foreach (var opt in element.Elements("option"))
			{
				result[opt.Attribute("key").Value] = opt.Value;
			}
			return result;
		}

		/// <summary>
		/// Saves a number of media objects to a Powerpraise portfolio file.
		/// As a portfolio file contains some settings besides the list of media objects, 
		/// these settings are filled with default values.
		/// TODO: move to a separate class
		/// </summary>
		/// <param name="enumerable">The media objects.</param>
		/// <param name="fileName">The target portfolio file.</param>
		public static void SavePortfolio(IEnumerable<Media> enumerable, string fileName)
		{ 
			XDocument doc = new XDocument(new XDeclaration("1.0","UTF-8","yes"));
			XElement root = new XElement("ppp", new XAttribute("version", "4.0"),
				new XElement("order",
					from m in enumerable
					select new XElement(
						"item",
						new XAttribute("mediatype", m.Uri.Scheme == "song" ? "powerpraise-song" : "file"),
						new XElement("file", GetMediaPathFromUri(m.Uri)),
						m.Options == null ? null : from opt in m.Options
							select new XElement("option", new XAttribute("key", opt.Key), opt.Value)
					)
				),
				new XElement("settings",
					new XComment("These settings are ignored in WordsLive"),
					new XElement("showonslide",
						new XElement("translation", "true"),
						new XElement("source", "true"),
						new XElement("cr", "true")),
					new XElement("blackbg", "false"),
					new XElement("transitions", 
						new XElement("enabled", "true"),
						new XElement("onbgchange", new XElement("steps", 40)),
						new XElement("onbgchange2", new XElement("steps", 10)),
						new XElement("onbeginend", new XElement("steps", 20)),
						new XElement("onbeginend2", new XElement("steps", 60)),
						new XElement("onslidechange", new XElement("steps", 40)),
						new XElement("onslidechange2", new XElement("steps", 20))),
					new XElement("master",
						new XElement("enabled", false),
						new XElement("masterfile"),
						new XElement("ovrfontformatting", false),
						new XElement("ovrtextpositioning", false),
						new XElement("ovrcopyright", false),
						new XElement("ovrsource", false),
						new XElement("ovroutlineshadow", false),
						new XElement("ovrbackground", false)
					)
				)
			);

			doc.Add(new XComment("This file was written using WordsLive"));
			doc.Add(root);

			StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8); 
			doc.Save(writer);
			writer.Close();
		}

		private static string GetMediaPathFromUri(Uri uri)
		{
			if (uri.Scheme == "song")
				return Uri.UnescapeDataString(uri.AbsolutePath).Substring(1);

			if (uri.IsFile)
				return uri.LocalPath;

			throw new NotImplementedException("Saving remote URIs not implemented yet.");
		}
	}
}
