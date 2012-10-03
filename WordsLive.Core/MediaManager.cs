using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WordsLive.Core.Songs;
using System.Xml.Linq;

namespace WordsLive.Core
{
	public class MediaEventArgs : EventArgs
	{
		public Media Media { get; private set; }

		public MediaEventArgs(Media media)
		{
			Media = media;
		}
	}

	public delegate void MediaEventHandler(object sender, MediaEventArgs args);

	public static class MediaManager
	{
		private static List<MediaFileHandler> fileHandlers = new List<MediaFileHandler>();

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

		public static IEnumerable<MediaFileHandler> FileHandlers
		{
			get
			{
				return fileHandlers;
			}
		}

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

				var notSupported = new UnsupportedMedia();
				notSupported.LoadMetadata(file.FullName);
				return notSupported;
			}
			else
			{
				var notFound = new FileNotFoundMedia();
				notFound.LoadMetadata(file.FullName);
				return notFound;
			}
		}

		public static Media LoadMediaMetadata(string file)
		{
			if (String.IsNullOrEmpty(file))
				throw new ArgumentException("file");

			return LoadMediaMetadata(new FileInfo(file));
		}

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
					media.LoadMetadata(file.FullName);
					return media;
				}
			}
			else
			{
				var notFound = new FileNotFoundMedia();
				notFound.LoadMetadata(file.FullName);
				return notFound;
			}
		}

		public static Media LoadMedia(Media media)
		{
			if (media == null)
				throw new ArgumentNullException("media");

			media.Load();
			OnMediaLoaded(media);
			return media;
		}

		public static event MediaEventHandler MediaLoaded;

		private static void OnMediaLoaded(Media media)
		{
			if (MediaLoaded != null)
				MediaLoaded(null, new MediaEventArgs(media));
		}

		public static IEnumerable<Media> LoadMultipleMediaMetadata(IEnumerable<string> fileName)
		{
			var files = from f in fileName select new FileInfo(f);

			// if not all of them exist load them seperately
			if (!files.All(f => f.Exists))
			{
				foreach (var file in files)
				{
					yield return LoadMediaMetadata(file);
				}
			}

			var extensions = (from f in files select f.Extension.ToLower()).Distinct();

			// select handlers that can handle all selected file types
			var handlers = from h in fileHandlers where !extensions.Except(h.Extensions).Any() select h;

			Media[] result;
			foreach (var h in handlers)
			{
				result = h.TryHandleMultiple(files.ToArray());
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

		private static string GetPowerpointTempPath()
		{
			return Path.Combine(Path.Combine(Path.GetTempPath(), "Powerpraise"), Path.GetRandomFileName()) + Path.DirectorySeparatorChar;
		}

		public static void InitDirectories(string songsDirectory, string backgroundsDirectory)
		{
			SongsDirectory = songsDirectory;
			if (SongsDirectory.EndsWith("\\"))
				SongsDirectory = SongsDirectory.Substring(0, SongsDirectory.Length - 1);

			BackgroundsDirectory = backgroundsDirectory;
			if (BackgroundsDirectory.EndsWith("\\"))
				BackgroundsDirectory = BackgroundsDirectory.Substring(0, BackgroundsDirectory.Length - 1);
		}

		public static string SongsDirectory { get; private set; }
		public static string BackgroundsDirectory { get; private set; }
	}
}
