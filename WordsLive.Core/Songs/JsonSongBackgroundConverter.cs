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
using Newtonsoft.Json;
using WordsLive.Core.Data;
using System.Drawing;
using System.IO;

namespace WordsLive.Core.Songs
{
	public class JsonSongBackgroundConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var bg = (SongBackground)value;
			writer.WriteStartObject();
			if (bg.Type == SongBackgroundType.Color)
			{
				var conv = new JsonColorConverter();
				writer.WritePropertyName("Color");
				conv.WriteJson(writer, bg.Color, serializer);
			}
			else
			{

				try
				{
					var uri = DataManager.Backgrounds.GetFile(bg).Uri;
					if (bg.Type == SongBackgroundType.Image)
					{
						writer.WritePropertyName("Image");
						writer.WriteValue(uri);
					}
					else
					{
						writer.WritePropertyName("Video");
						writer.WriteValue(uri);
					}
				}
				catch (FileNotFoundException)
				{
					var conv = new JsonColorConverter();
					writer.WritePropertyName("Color");
					conv.WriteJson(writer, Color.Black, serializer);
				}
			}

			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(SongBackground).IsAssignableFrom(objectType);
		}
	}
}
