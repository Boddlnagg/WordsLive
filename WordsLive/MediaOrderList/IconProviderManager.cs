/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
using System.Linq;
using WordsLive.Core;

namespace WordsLive.MediaOrderList
{
	public class IconProviderManager
	{
		internal IconProviderManager()
		{
		}

		private Dictionary<Type, Type> providers = new Dictionary<Type, Type>();

		public void RegisterProvider<TMedia, TProvider>()
			where TMedia : Media
			where TProvider : IconProvider
		{
			RegisterProvider(typeof(TMedia), typeof(TProvider));
		}

		private void RegisterProvider(Type mediaType, Type providerType)
		{
			providers.Add(mediaType, providerType);
		}

		public void RegisterProvidersFromTypes(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				if (typeof(IconProvider).IsAssignableFrom(type))
				{
					var attributes = type.GetCustomAttributes(typeof(TargetMediaAttribute), true).Cast<TargetMediaAttribute>();
					foreach (var attr in attributes)
					{
						RegisterProvider(attr.Type, type);
					}
				}
			}
		}

		public IconProvider CreateProvider(Media data)
		{
			if (!providers.ContainsKey(data.GetType()))
			{
				return new IconProvider(data);
			}
			else
			{
				return (IconProvider)System.Activator.CreateInstance(providers[data.GetType()], data);
			}
		}
	}
}
