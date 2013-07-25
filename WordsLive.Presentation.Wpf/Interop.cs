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
using System.Runtime.InteropServices;

namespace WordsLive.Presentation.Wpf
{
	// Mostly taken from http://www.11011.net/archives/000653.html
	public static class Interop
	{
		public struct Point
		{
			public int x;
			public int y;
		}

		public struct Size
		{
			public int Width, Height;
		}

		public struct ThumbnailProperties
		{
			public ThumbnailFlags Flags;
			public Rect Destination;
			public Rect Source;
			public byte Opacity;
			public bool Visible;
			public bool SourceClientAreaOnly;
		}

		public struct Rect
		{
			public Rect(int x, int y, int x1, int y1)
			{
				this.Left = x;
				this.Top = y;
				this.Right = x1;
				this.Bottom = y1;
			}

			public int Left, Top, Right, Bottom;
		}

		[Flags]
		public enum ThumbnailFlags : int
		{
			RectDestination = 1,
			RectSource = 2,
			Opacity = 4,
			Visible = 8,
			SourceClientAreaOnly = 16
		}

		[Flags]
		private enum DwmWindowAttribute
		{
			NCRenderingEnabled = 1,
			NCRenderingPolicy,
			TransitionsForceDisabled,
			AllowNCPaint,
			CaptionButtonBounds,
			NonClientRtlLayout,
			ForceIconicRepresentation,
			Flip3DPolicy,
			ExtendedFrameBounds,
			HasIconicBitmap,
			DisallowPeek,
			ExcludedFromPeek,
			Last
		}


		//[DllImport("user32.dll")]
		//internal static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwmAttribute, IntPtr pvAttribute, uint cbAttribute);

		[DllImport("dwmapi.dll")]
		private static extern int DwmIsCompositionEnabled(out bool result);

		[DllImport("dwmapi.dll")]
		internal static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr source, out IntPtr hthumbnail);

		[DllImport("dwmapi.dll")]
		internal static extern int DwmUnregisterThumbnail(IntPtr HThumbnail);

		[DllImport("dwmapi.dll")]
		internal static extern int DwmUpdateThumbnailProperties(IntPtr HThumbnail, ref ThumbnailProperties props);

		[DllImport("dwmapi.dll")]
		internal static extern int DwmQueryThumbnailSourceSize(IntPtr HThumbnail, out Size size);

		public static bool IsDwmEnabled
		{
			get
			{
				if (Environment.OSVersion.Version.Major >= 6)
				{
					bool enabled;
					DwmIsCompositionEnabled(out enabled);
					return enabled;
				}

				return false;
			}
		}

		public static void RemoveFromAeroPeek(IntPtr Hwnd)
		{
			if (IsDwmEnabled)
			{
				var status = Marshal.AllocHGlobal(sizeof(int));
				Marshal.WriteInt32(status, 1); // true
				DwmSetWindowAttribute(Hwnd, DwmWindowAttribute.ExcludedFromPeek, status, sizeof(int));
				Marshal.FreeHGlobal(status);
			}
		}
	}
}
