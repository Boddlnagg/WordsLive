using System;
using System.Windows;

namespace WordsLive.Presentation.Wpf
{
	public class LiveWindowPreviewProvider : WpfPreviewProvider
	{
		WindowThumbnail thumb = null;

		public LiveWindowPreviewProvider(IntPtr windowHandle)
		{
			if (Interop.IsDwmEnabled)
			{
				thumb = new WindowThumbnail();
				thumb.Source = windowHandle;
				thumb.ClientAreaOnly = true;
			}
		}

		protected override UIElement WpfControl
		{
			get
			{
				return thumb;
			}
		}
	}
}
