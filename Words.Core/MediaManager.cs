using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Words.Core.Songs;
using System.Xml.Linq;

namespace Words.Core
{
	public static class MediaManager
	{
		private static Dictionary<string, MediaType> mediaFileExtensions = new Dictionary<string, MediaType>();
		private static List<MediaType> mediaTypes = new List<MediaType>();

		public static IEnumerable<MediaType> MediaTypes
		{
			get
			{
				return mediaTypes;
			}
		}

		private static void RegisterMediaFileExtensions(MediaType type)
		{
			foreach (var ext in type.Extensions)
			{
				string key = ext.ToLower();
				key = key.StartsWith(".") ? key : "." + key;
				if (mediaFileExtensions.ContainsKey(key))
				{
					// TODO (Core): what to do? maybe register anyway and allow to choose from the registered media types.
					throw new ArgumentException("File extension '" + key + "' already registered.");
				}

				mediaFileExtensions.Add(key, type);
			}
		}

		public static void RegisterMediaTypes(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				if (type.IsSubclassOf(typeof(Media)))
				{
					var attr = type.GetCustomAttributes(typeof(MediaTypeAttribute), true).Cast<MediaTypeAttribute>().FirstOrDefault();
					if (attr != null)
					{
						var t = new MediaType(attr.Description, attr.Extensions, type);
						RegisterMediaFileExtensions(t);
						mediaTypes.Add(t);
					}
				}
			}
		}

		public static Media LoadMediaMetadata(string fileName)
		{
			FileInfo file = new FileInfo(fileName);
			Media m;

			if (file.Exists)
			{
				string ext = file.Extension.ToLower();
				if (mediaFileExtensions.ContainsKey(ext))
				{
					m = mediaFileExtensions[ext].CreateInstance();
				}
				else
				{
					m = new UnsupportedMedia();
				}
			}
			else
			{
				m = new FileNotFoundMedia();
			}

			m.LoadMetadata(file.FullName);
			return m;
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
					if (root.Attribute("version").Value == "3.0")
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
			XElement root = new XElement("ppp", new XAttribute("version", "3.0"),
				new XElement("order",
					from m in enumerable
					select new XElement(
						"item",
						new XAttribute("mediatype", m is Song ? "powerpraise-song" : "unknown"),
						new XElement("file", m is Song ? m.File.Replace(SongsDirectory + Path.DirectorySeparatorChar, "") : m.File)
					)
				),
				new XElement("settings",
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
						new XElement("enabled", "false"),
						new XElement("masterfile", "Standard.ppl"),
						new XElement("ovrfontformatting", false),
						new XElement("ovrtextpositioning", false),
						new XElement("ovrcopyright", false),
						new XElement("ovrsource", false),
						new XElement("ovroutlineshadow", false),
						new XElement("ovrbackground", false)
					)
				)
			);

			doc.Add(new XComment("This file was written using Words"));
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
