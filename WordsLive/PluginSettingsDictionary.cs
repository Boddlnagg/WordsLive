using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace WordsLive
{
	public class PluginSettingsDictionary : IXmlSerializable
	{
		private Dictionary<string, object> dictionary = new Dictionary<string, object>();

		public bool Contains(string ns, string key)
		{
			if (string.IsNullOrWhiteSpace(ns))
				throw new ArgumentException("ns");

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("key");

			key = ns + ":" + key;
			return dictionary.ContainsKey(key);
		}

		public T Get<T>(string ns, string key)
		{
			if (string.IsNullOrWhiteSpace(ns))
				throw new ArgumentException("ns");

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("key");

			key = ns + ":" + key;
			return (T)dictionary[key];
		}

		public void Set<T>(string ns, string key, T value)
		{
			if (string.IsNullOrWhiteSpace(ns))
				throw new ArgumentException("ns");

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("key");

			key = ns + ":" + key;
			dictionary[key] = value;
		}

		public void Unset(string ns, string key)
		{
			if (string.IsNullOrWhiteSpace(ns))
				throw new ArgumentException("ns");

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("key");

			key = ns + ":" + key;
			dictionary.Remove(key);
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			XmlSerializer keySerializer = new XmlSerializer(typeof(string));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(object));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");

				reader.ReadStartElement("key");
				string key = (string)keySerializer.Deserialize(reader);
				reader.ReadEndElement();
				reader.ReadStartElement("value");
				object value = (object)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				dictionary.Add(key, value);

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			XmlSerializer keySerializer = new XmlSerializer(typeof(string));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(object));

			foreach (string key in dictionary.Keys)
			{
				writer.WriteStartElement("item");

				writer.WriteStartElement("key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement("value");
				object value = dictionary[key];
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
	}
}
