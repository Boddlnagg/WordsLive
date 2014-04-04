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
