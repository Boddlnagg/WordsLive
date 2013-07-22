/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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
using Awesomium.Core;
using Awesomium.Windows.Controls;
using WordsLive.Presentation;
using WordsLive.Presentation.Wpf;
using WordsLive.Resources;

namespace WordsLive.Awesomium
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

			this.Control.Web.ShowCreatedWebView += OnShowCreatedWebView;
			this.Control.Web.Crashed += OnWebviewCrashed;

			// we only need to disable input, when manual updates are disabled
			if (!enableInput && !manualUpdate)
				(this.Control.Web as WebControl).ProcessInput = ViewInput.None;

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

		void OnShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
		{
			throw new NotImplementedException();
			// no support for multiple browser instances/tabs/windows, so open external links in the same window
			//if (e.Url.Length > 0)
			//{
			//	this.Control.Web.LoadURL(e.Url);
			//}
		}

		private void OnWebviewCrashed(object sender, EventArgs e)
		{
			var result = MessageBox.Show(Resource.vAwesomiumProcessCrashed, Resource.vAwesomiumProcessCrashedTitle, MessageBoxButton.OKCancel);

			if (result == MessageBoxResult.OK)
			{
				this.Control.Web.Dispose();
				Controller.ReloadActiveMedia();
			}
		}

		public override void Close()
		{
			Control.Web.ShowCreatedWebView -= OnShowCreatedWebView;
			Control.Web.Crashed -= OnWebviewCrashed;

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
