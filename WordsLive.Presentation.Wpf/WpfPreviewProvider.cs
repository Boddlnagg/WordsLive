using System.Windows;
using System.Windows.Forms.Integration;
using WinForms = System.Windows.Forms;

namespace WordsLive.Presentation.Wpf
{
	public abstract class WpfPreviewProvider : IPreviewProvider
	{
		private ElementHost cachedHost;

		public WinForms.Control PreviewControl
		{
			get
			{
				if (WpfControl == null)
					return null;

				if (cachedHost == null)
				{
					this.cachedHost = new ElementHost();
					this.cachedHost.Dock = System.Windows.Forms.DockStyle.Fill;
					this.cachedHost.Child = WpfControl;
				}

				return this.cachedHost;
			}
		}

		public UIElement WpfPreviewControl
		{
			get
			{
				if (cachedHost != null)
				{
					cachedHost.Child = null; // this prevents the control from being child to two visual parents
					cachedHost = null;
				}

				return WpfControl;
			}
		}

		public bool IsPreviewAvailable
		{
			get { return WpfControl != null; }
		}

		protected abstract UIElement WpfControl { get; }
	}
}

