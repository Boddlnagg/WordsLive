using System;
using System.Collections.Generic;
using System.Linq;
using Words.Core;

namespace Words
{
	public class ControlPanelManager
	{
		internal ControlPanelManager()
		{
		}

		private Dictionary<Type, Type> panelTypes = new Dictionary<Type, Type>();

		public void RegisterPanelType<TMedia, TPanel>()
			where TMedia : Media
			where TPanel : IMediaControlPanel
		{
			RegisterPanelType(typeof(TMedia), typeof(TPanel));
		}

		private void RegisterPanelType(Type mediaType, Type panelType)
		{
			panelTypes.Add(mediaType, panelType);
		}

		public void RegisterPanelsFromTypes(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				if (typeof(IMediaControlPanel).IsAssignableFrom(type))
				{
					var attributes = type.GetCustomAttributes(typeof(TargetMediaAttribute), true).Cast<TargetMediaAttribute>();
					foreach (var attr in attributes)
					{
						RegisterPanelType(attr.Type, type);
					}
				}
			}
		}

		public IMediaControlPanel CreatePanel(Media data)
		{
			Type dataType = data.GetType();
			Type panelType = null;

			if (panelTypes.ContainsKey(dataType))
			{
				panelType = panelTypes[dataType];
			}
			else
			{
				foreach (var key in panelTypes.Keys)
				{
					if (dataType.IsSubclassOf(key))
					{
						panelType = panelTypes[key];
						break;
					}
				}
			}
				
			if (panelType != null)
			{
				IMediaControlPanel panel = (IMediaControlPanel)System.Activator.CreateInstance(panelType); ;
				panel.Init(data);
				return panel;
			}
			else
			{
				throw new ArgumentException("No panel type registered for " + dataType.Name);
			}
		}
	}
}
