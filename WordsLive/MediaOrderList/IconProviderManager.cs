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
