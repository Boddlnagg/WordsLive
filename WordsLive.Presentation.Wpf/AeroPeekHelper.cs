using System;
using System.Runtime.InteropServices;

namespace Words.Presentation.Wpf
{
	public class AeroPeekHelper
	{
		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwmAttribute, IntPtr pvAttribute, uint cbAttribute);
		
		[DllImport("dwmapi.dll")]
		private static extern int DwmIsCompositionEnabled(out bool result);

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
