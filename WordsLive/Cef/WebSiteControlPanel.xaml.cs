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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Core;

namespace WordsLive.Cef
{
	/// <summary>
	/// This is currently not used (handler is disabled
	/// </summary>
	[TargetMedia(typeof(WebSite))]
	public partial class WebSiteControlPanel : UserControl, IMediaControlPanel
	{
		private CefPresentation presentation;
		
		public WebSiteControlPanel()
		{
			InitializeComponent();
		}

		private WordsLive.Core.WebSite website;

		public Control Control
		{
			get { return this; }
		}

		public Core.Media Media
		{
			get { return website; }
		}

		public void Init(Core.Media media)
		{
			website = media as WordsLive.Core.WebSite;

			if (website == null)
				throw new ArgumentException("media must not be null and of type WebSite");

			this.urlTextBox.Text = website.Url;

			presentation = Controller.PresentationManager.CreatePresentation<CefPresentation>();
			presentation.Load(true);

			(presentation.Control.Web as CefSharp.Wpf.ChromiumWebBrowser).AddressChanged += Web_AddressChanged;
		}

		private void Web_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Dispatcher.BeginInvoke(new Action(() =>
			{
				this.urlTextBox.Text = presentation.Control.Web.Address;
			}));
		}

		private void goButton_Click(object sender, RoutedEventArgs e)
		{
			string url = urlTextBox.Text;
			if (!url.Contains("://"))
				url = "http://" + url;

			presentation.Control.Web.Load(url);
			Controller.PresentationManager.CurrentPresentation = presentation;
		}

		public bool IsUpdatable
		{
			get { return false; }
		}

		public ControlPanelLoadState LoadState
		{
			get { return ControlPanelLoadState.Loaded; }
		}

		public void Close()
		{
			if (presentation.Control.Web != null)
				(presentation.Control.Web as CefSharp.Wpf.ChromiumWebBrowser).AddressChanged -= Web_AddressChanged;

			if (Controller.PresentationManager.CurrentPresentation != presentation)
				presentation.Close();
		}

		private void urlTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				goButton_Click(this, new RoutedEventArgs());
			}
		}

		private void buttonBack_Click(object sender, RoutedEventArgs e)
		{
			(presentation.Control.Web as CefSharp.Wpf.ChromiumWebBrowser).BackCommand.Execute(null);
		}

		private void buttonForward_Click(object sender, RoutedEventArgs e)
		{
			(presentation.Control.Web as CefSharp.Wpf.ChromiumWebBrowser).ForwardCommand.Execute(null);
		}
	}
}
