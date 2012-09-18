using System;
using WinForms = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using System.Windows;

namespace Words.Presentation.Wpf
{
	public class CloningWpfPreviewProvider<T> : WpfPreviewProvider where T : FrameworkElement, new()
	{
		private Rectangle previewRectangle;
		
		
		public CloningWpfPreviewProvider(T control)
		{
			var brush = new System.Windows.Media.VisualBrush(control);
			this.previewRectangle = new System.Windows.Shapes.Rectangle();
			this.previewRectangle.Fill = brush;
		}
		
		protected override UIElement WpfControl
		{
			get
			{
				return this.previewRectangle;
			}
		}
	}
}
