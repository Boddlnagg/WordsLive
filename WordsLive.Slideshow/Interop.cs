using System;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WordsLive.Slideshow
{
	public class Interop
	{
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		protected static extern bool DeleteObject(IntPtr hObject);

		public static BitmapSource ConvertBitmap(Bitmap bmp)
		{
			BitmapSource result;
			IntPtr hBitmap = bmp.GetHbitmap();
			try
			{
				result = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
					IntPtr.Zero, System.Windows.Int32Rect.Empty,
					BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));

				result.Freeze();
			}
			finally
			{
				DeleteObject(hBitmap);
				bmp.Dispose();
			}

			return result;
		}
	}
}
