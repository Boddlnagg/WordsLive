using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using System;
using Words.Presentation.Wpf;

namespace Words
{
	public class AwesomiumPresentation : WpfPresentation<AwesomiumWrapper>
	{
		Window win;

		public new WebControl Control
		{
			get
			{
				return base.Control.Web;
			}
		}

		public Grid BackgroundGrid
		{
			get
			{
				return base.Control.BackgroundGrid;
			}
		}

		public void Load(bool enableInput)
		{
			AwesomiumManager.Register(this.Control);

			this.Control.OpenExternalLink += OnOpenExternalLink;
			this.Control.Crashed += OnWebviewCrashed;

			if (!enableInput)
				this.Control.DeferInput(); // TODO: is this the only possible solution to disable input?

			win = new Window();
			win.Width = this.Area.WindowSize.Width;
			win.Height = this.Area.WindowSize.Height;
			win.Content = base.Control;
			win.Top = -10000;
			win.Left = -10000;
			win.Focusable = false;
			win.ShowActivated = false;
			win.ShowInTaskbar = false;
			win.WindowStyle = WindowStyle.None; // no window border
			win.ResizeMode = ResizeMode.NoResize;
			win.Show();
		}

		public override void Show(int transitionDuration = 0, Action callback = null)
		{
			if (win != null)
			{
				win.Content = null;
				win.Close();
				win = null;
			}
			base.Show(transitionDuration, callback);
		}

		private void OnOpenExternalLink(object sender, OpenExternalLinkEventArgs e)
		{
			// no support for multiple browser instances/tabs/windows, so open external links in the same window
			if (e.Url.Length > 0)
			{
				this.Control.LoadURL(e.Url);
			}
		}

		private void OnWebviewCrashed(object sender, EventArgs e)
		{
			var result = MessageBox.Show("Der Anzeigeprozess Words.Awesomium.exe wurde beendet oder ist abgestürzt. Words wird versuchen, die aktuelle Präsentation neu zu laden um die Anzeige wiederherzustellen.",
				"Fehler", MessageBoxButton.OKCancel);			

			if (result == MessageBoxResult.OK)
				Controller.ReloadActiveMedia();
		}

		public override void Close()
		{
			base.Close();
			if (win != null)
			{
				win.Dispatcher.Invoke(new Action(() => win.Close()));
			}
			AwesomiumManager.Close(this.Control);
		}

		public bool IsTransparent
		{
			get
			{
				return this.Control.IsTransparent;
			}
			set
			{
				this.Control.FlushAlpha = !value;
				this.Control.IsTransparent = value;
			}
		}
	}
}
