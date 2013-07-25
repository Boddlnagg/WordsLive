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

namespace WordsLive.Slideshow
{
	public static class Settings
	{
		public static readonly string Namespace = "Slideshow";

		//public static bool EnableLivePreview
		//{
		//	get
		//	{
		//		if (!Controller.PluginSettings.Contains(ns, "EnableLivePreview"))
		//		{
		//			// enable by default if OS is Vista or newer
		//			bool enable = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;
		//			Controller.PluginSettings.Set(ns, "EnableLivePreview", enable);
		//		}

		//		return Controller.PluginSettings.Get<bool>(ns, "EnableLivePreview");
		//	}
		//	set
		//	{
		//		Controller.PluginSettings.Set(ns, "EnableLivePreview", value);
		//	}
		//}
	}
}
