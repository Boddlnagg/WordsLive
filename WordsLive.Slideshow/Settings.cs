using System;

namespace WordsLive.Slideshow
{
	public static class Settings
	{
		private const string ns = "Slideshow";

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
