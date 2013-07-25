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

namespace WordsLive
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
				IMediaControlPanel panel = (IMediaControlPanel)System.Activator.CreateInstance(panelType);
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
