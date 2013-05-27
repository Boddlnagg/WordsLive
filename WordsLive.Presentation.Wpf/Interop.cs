using System;
using System.Runtime.InteropServices;

namespace WordsLive.Presentation.Wpf
{
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
			RectDetination = 1,
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
		

		public static void RemoveFromAeroPeek(IntPtr Hwnd)
		{
			if (Environment.OSVersion.Version.Major >= 6)
			{
				bool enabled;
				DwmIsCompositionEnabled(out enabled);
				if (enabled)
				{
					var status = Marshal.AllocHGlobal(sizeof(int));
					Marshal.WriteInt32(status, 1); // true
					DwmSetWindowAttribute(Hwnd, DwmWindowAttribute.ExcludedFromPeek, status, sizeof(int));
					Marshal.FreeHGlobal(status);
				}
			}
		}
	}
}
