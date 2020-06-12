/*
 * WordsLive - worship projection software
 * Copyright (c) 2020 Patrick Reisert
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
using System.Windows;
using WordsLive.Presentation;
using WordsLive.Presentation.Wpf;
using WordsLive.Resources;

namespace WordsLive.Cef
{
	public class CefPresentation : WpfPresentation<CefWrapper>
	{
		Window win;

		public void Load(bool enableInput, bool manualUpdate = false)
		{
			if (!manualUpdate)
				this.Control.Load(false, null);
			else
				this.Control.Load(true, this.Area);

			this.Control.Web.RequestHandler = new CefRequestHandler();
			(this.Control.Web.RequestHandler as CefRequestHandler).RenderProcessTerminated += OnRenderProcessTerminated;

			// we only need to disable input, when manual updates are disabled
			if (!enableInput && !manualUpdate)
				(this.Control.Web as CefSharp.Wpf.ChromiumWebBrowser).IsEnabled = false;

			// create an offscreen window to render the web content
			win = new Window();
			win.Owner = WordsLive.Presentation.Wpf.WpfPresentationWindow.Instance.Owner;
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

		private void OnRenderProcessTerminated(object sender, CefRequestHandler.RenderProcessTerminatedEventArgs e)
		{
			Controller.DispatchToMainWindow(() =>
			{
				var result = MessageBox.Show(Resource.vDisplayProcessCrashed, Resource.vDisplayProcessCrashedTitle, MessageBoxButton.OKCancel);

				if (result == MessageBoxResult.OK)
				{
					e.ChromiumWebBrowser.Dispose();
					Controller.ReloadActiveMedia();
				}
			});
		}

		public override void Show(int transitionDuration = 0, Action callback = null, IPresentation previous = null)
		{
			if (win != null)
			{
				win.Content = null;
				// we can't close the window here already, because it will dispose of the CEF control that's
				// somehow still connected to that window (maybe a CefSharp bug?)
			}
			base.Show(transitionDuration, callback, previous);
		}

		public override void Close()
		{
			if (win != null)
			{
				win.Close();
				win = null;
			}

			if (this.Control.Web.RequestHandler is CefRequestHandler)
				(this.Control.Web.RequestHandler as CefRequestHandler).RenderProcessTerminated -= OnRenderProcessTerminated;

			base.Close();
			this.Control.Close();
			if (win != null)
			{
				win.Dispatcher.Invoke(new Action(() => win.Close()));
			}
			this.Control.Web.Dispose();
		}
	}
}
