using System.Windows;
using System.Windows.Shapes;

namespace WordsLive.Presentation.Wpf
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
