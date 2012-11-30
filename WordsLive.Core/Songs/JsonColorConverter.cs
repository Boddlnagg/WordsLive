using System;
using System.Drawing;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs
{
	public class JsonColorConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var color = (Color)value;
			writer.WriteStartArray();
			writer.WriteValue(color.R);
			writer.WriteValue(color.G);
			writer.WriteValue(color.B);
			writer.WriteValue(color.A);
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(Color).IsAssignableFrom(objectType);
		}
	}
}
