using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WordsLive.Core.Songs;
using System.Xml.Linq;

namespace WordsLive.Core
{
	/// <summary>
	/// Manages the available media types and file handlers.
	/// </summary>
	public static class MediaManager
	{
		private static List<MediaFileHandler> fileHandlers = new List<MediaFileHandler>();

		/// <summary>
		/// Registers file handlers from the given types. Any non-abstract subclass of <see cref="MediaFileHandler"/>
		/// that is found will be registered.
		/// </summary>
		/// <param name="types"></param>
		public static void RegisterHandlersFromTypes(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				if (type.IsClass && !type.IsAbstract && typeof(MediaFileHandler).IsAssignableFrom(type))
				{
					MediaFileHandler handler = (MediaFileHandler)Activator.CreateInstance(type);
					fileHandlers.Add(handler);
				}
			}
		}

		/// <summary>
		/// Gets the registered media file handlers.
		/// </summary>
		public static IEnumerable<MediaFileHandler> FileHandlers
		{
			get
			{
				return fileHandlers;
			}
		}

		/// <summary>
		/// Creates a media object from a file. If the file doesn't exist, a <see cref="FileNotFoundMedia"/> is returned.
		/// If no appropiate file handler is found, a <see cref="UnsupportedMedia"/> is returned.
		/// Otherwise the correct <see cref="Media"/> type is returned.
		/// </summary>
		/// <param name="file">The <see cref="FileInfo"/> with the path.</param>
		/// <returns>A <see cref="Media"/> object.</returns>
		public static Media LoadMediaMetadata(FileInfo file)
		{
			if (file.Exists)
			{
				string ext = file.Extension.ToLower();
				var handlers = from h in fileHandlers where h.Extensions.Contains(ext) select h;
				Media result;
				foreach (var h in handlers)
				{
					result = h.TryHandle(file);
					if (result != null)
						return result;
				}

				return new UnsupportedMedia(file.FullName);
			}
			else
			{
				return new FileNotFoundMedia(file.FullName);
			}
		}

		/// <summary>
		/// Creates a media object from a file. If the file doesn't exist, a <see cref="FileNotFoundMedia"/> is returned.
		/// If no appropiate file handler is found, a <see cref="UnsupportedMedia"/> is returned.
		/// Otherwise the correct <see cref="Media"/> type is returned.
		/// </summary>
		/// <param name="file">The full path as a string.<param>
		/// <returns>A <see cref="Media"/> object.</returns>
		public static Media LoadMediaMetadata(string file)
		{
			if (String.IsNullOrEmpty(file))
				throw new ArgumentException("file");

			return LoadMediaMetadata(new FileInfo(file));
		}

		/// <summary>
		/// Loads multiple files at once, trying to call a single handler's <see cref="MediaFileHandler.TryHandleMultiple"/> method
		/// in order to load them into a single media object if supported.
		/// </summary>
		/// <param name="files">The paths to the files to load.</param>
		/// <returns>The loaded media objects, either one per file or less.</returns>
		public static IEnumerable<Media> LoadMultipleMediaMetadata(IEnumerable<string> files)
		{
			var fileInfos = from f in files select new FileInfo(f);

			// if not all of them exist load them seperately
			if (!fileInfos.All(f => f.Exists))
			{
				foreach (var file in files)
				{
					yield return LoadMediaMetadata(file);
				}
			}

			var extensions = (from f in fileInfos select f.Extension.ToLower()).Distinct();

			// select handlers that can handle all selected file types
			var handlers = from h in fileHandlers where !extensions.Except(h.Extensions).Any() select h;

			IEnumerable<Media> result;
			foreach (var h in handlers)
			{
				result = h.TryHandleMultiple(fileInfos);
				if (result != null)
				{
					foreach (var r in result)
						yield return r;

					yield break;
				}
			}

			// if not all of them are supported by a single handler load them seperately
			foreach (var file in files)
			{
				yield return LoadMediaMetadata(file);
			}
		}

		/// <summary>
		/// Reloads the metadata of a media object.
		/// </summary>
		/// <param name="media">The object to reload.</param>
		/// <returns>The reloaded media object. Possibly but not necessarily the same object that was given as input.</returns>
		public static Media ReloadMediaMetadata(Media media)
		{
			var file = new FileInfo(media.File);
			if (file.Exists)
			{
				if (media is FileNotFoundMedia)
				{
					return LoadMediaMetadata(file);
				}
				else
				{
					media.ReloadMetadata();
					return media;
				}
			}
			else
			{
				return new FileNotFoundMedia(file.FullName);
			}
		}

		/// <summary>
		/// Loads a given media object by calling it's <see cref="Media.Load"/> method and
		/// triggering the <see cref="MediaLoaded"/> event.
		/// </summary>
		/// <param name="media"></param>
		/// <returns></returns>
		public static Media LoadMedia(Media media)
		{
			if (media == null)
				throw new ArgumentNullException("media");

			media.Load();
			OnMediaLoaded(media);
			return media;
		}

		/// <summary>
		/// This event is called when a media object was loaded.
		/// </summary>
		public static event EventHandler<MediaEventArgs> MediaLoaded;

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
													select i.Attribute("mediatype").Value == "powerpraise-song" ?
													LoadMediaMetadata(Path.Combine(SongsDirectory, i.Element("file").Value)) :
													LoadMediaMetadata(i.Element("file").Value))
						{
							yield return m;
						}

						yield break;
					}
					else if (root.Attribute("version").Value == "2.2")
					{
						foreach (Media m in from i in root.Elements("item")
													select MediaManager.LoadMediaMetadata(Path.Combine(SongsDirectory, i.Element("file").Value)))
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
			XDocument doc = new XDocument(new XDeclaration("1.0","ISO-8859-1","yes"));
			XElement root = new XElement("ppp", new XAttribute("version", "4.0"),
				new XElement("order",
					from m in enumerable
					select new XElement(
						"item",
						new XAttribute("mediatype", m is Song ? "powerpraise-song" : "file"),
						new XElement("file", m is Song ? m.File.Replace(SongsDirectory + Path.DirectorySeparatorChar, "") : m.File)
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

			StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("iso-8859-1"));
			doc.Save(writer);
			writer.Close();
		}

		/// <summary>
		/// Initializes the directories for songs and backgrounds.
		/// </summary>
		/// <param name="songsDirectory">The directory for songs.</param>
		/// <param name="backgroundsDirectory">The directory for backgrounds.</param>
		public static void InitDirectories(string songsDirectory, string backgroundsDirectory)
		{
			SongsDirectory = songsDirectory;
			if (SongsDirectory.EndsWith("\\"))
				SongsDirectory = SongsDirectory.Substring(0, SongsDirectory.Length - 1);

			BackgroundsDirectory = backgroundsDirectory;
			if (BackgroundsDirectory.EndsWith("\\"))
				BackgroundsDirectory = BackgroundsDirectory.Substring(0, BackgroundsDirectory.Length - 1);
		}

		/// <summary>
		/// Gets the directory for songs.
		/// </summary>
		public static string SongsDirectory { get; private set; }

		/// <summary>
		/// Gets the directory for backgrounds.
		/// </summary>
		public static string BackgroundsDirectory { get; private set; }
	}
}
