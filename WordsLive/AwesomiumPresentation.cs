using System;
using System.Windows;
using Awesomium.Core;
using WordsLive.Presentation;
using WordsLive.Presentation.Wpf;

namespace WordsLive
{
	public class AwesomiumPresentation : WpfPresentation<AwesomiumWrapper>
	{
		Window win;

		public void Load(bool enableInput, bool manualUpdate = false)
		{
			if (!manualUpdate)
				this.Control.Load(false, null);
			else
				this.Control.Load(true, this.Area);

			this.Control.Web.OpenExternalLink += OnOpenExternalLink;
			this.Control.Web.Crashed += OnWebviewCrashed;

			// we only need to disable input, when manual updates are disabled
			if (!enableInput && !manualUpdate)
				this.Control.Web.DeferInput(); // TODO: is this the only possible solution to disable input?

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

		public override void Show(int transitionDuration = 0, Action callback = null, IPresentation previous = null)
		{
			if (win != null)
			{
				win.Content = null;
				win.Close();
				win = null;
			}
			base.Show(transitionDuration, callback, previous);
		}

		private void OnOpenExternalLink(object sender, OpenExternalLinkEventArgs e)
		{
			// no support for multiple browser instances/tabs/windows, so open external links in the same window
			if (e.Url.Length > 0)
			{
				this.Control.Web.LoadURL(e.Url);
			}
		}

		private void OnWebviewCrashed(object sender, EventArgs e)
		{
			var result = MessageBox.Show("Der Anzeigeprozess WordsLive.Awesomium.exe wurde beendet oder ist abgestürzt. WordsLive wird versuchen, die aktuelle Präsentation neu zu laden um die Anzeige wiederherzustellen.", // TODO: localize
				"Fehler", MessageBoxButton.OKCancel);

			if (result == MessageBoxResult.OK)
				Controller.ReloadActiveMedia();
		}

		public override void Close()
		{
			base.Close();
			this.Control.Close();
			if (win != null)
			{
				win.Dispatcher.Invoke(new Action(() => win.Close()));
			}
			AwesomiumManager.Close(this.Control.Web);
		}

		public bool IsTransparent
		{
			get
			{
				return this.Control.Web.IsTransparent;
			}
			set
			{
				this.Control.Web.FlushAlpha = !value;
				this.Control.Web.IsTransparent = value;
			}
		}
	}
}
