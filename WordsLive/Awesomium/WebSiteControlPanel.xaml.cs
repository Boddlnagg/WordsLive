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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Core;

namespace WordsLive.Awesomium
{
	[TargetMedia(typeof(WebSite))]
	public partial class WebSiteControlPanel : UserControl, IMediaControlPanel
	{
		private AwesomiumPresentation presentation;
		private bool isBible;

		public class BibleTranslation
		{
			public static readonly BibleTranslation LastUsed = new BibleTranslation("(Zuletzt verwendet)", "");

			public string Name { get; private set; }
			public string Abbreviation { get; private set; }

			public BibleTranslation(string name, string abbr)
			{
				this.Name = name;
				this.Abbreviation = abbr;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		private List<BibleTranslation> bibleTranslations = new List<BibleTranslation>
		{
			BibleTranslation.LastUsed,
			new BibleTranslation("Luther 1984", "LUT"),
			new BibleTranslation("Elberfelder Bibel", "ELB"),
			new BibleTranslation("Hoffnung für alle", "HFA"),
			new BibleTranslation("Schlachter 2000", "SLT"),
			new BibleTranslation("Neue Genfer Übersetzung", "NGÜ"),
			new BibleTranslation("Gute Nachricht Bibel", "GNB"),
			new BibleTranslation("Einheitsübersetzung", "EU"),
			new BibleTranslation("Neues Leben. Die Bibel", "NLB"),
			new BibleTranslation("Neue evangelistische Übersetzung", "NeÜ"),
			new BibleTranslation("Englisch Standard Version", "ESV"),
			new BibleTranslation("New International Version", "NIV"),
			new BibleTranslation("Today's New International Version", "TNIV"),
			new BibleTranslation("New Int. Readers Version", "NIRV"),
			new BibleTranslation("King James Version", "KJV"),
			new BibleTranslation("Bible du Semeur", "BDS"),
			new BibleTranslation("Segond 21", "S21"),
			new BibleTranslation("La Parole è Vita", "ITA")
			// TODO: add more (or load them automatically)
		};

		public WebSiteControlPanel()
		{
			InitializeComponent();

			translationComboBox.DataContext = bibleTranslations;
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

			presentation = Controller.PresentationManager.CreatePresentation<AwesomiumPresentation>();
			presentation.Load(true);
			presentation.Control.Web.DocumentReady += (sender, args) =>
			{
				if (isBible)
				{
					var result = presentation.Control.Web.ExecuteJavascriptWithResult("(document.getElementById('searchSelect') != null) ? document.getElementById('searchSelect').children[1].children[0].attributes['value'].value : 'NULL'");
					if (result.ToString() != "NULL")
						bibleTextBox.Text = result.ToString();

					// TODO: maybe use the following when it's supported:
					// document.getElementById('pageMain').style.webkitHyphens = 'auto';
					// document.getElementById('pageMain').style.textAlign = 'justify';

					// adjust style and scroll selection into view (this is not done automatically by mobile version)
					presentation.Control.Web.ExecuteJavascript(@"
					document.getElementById('pageMain').style.fontSize = '15pt';
					document.getElementById('pageMain').style.marginLeft = '3%';
					document.getElementById('pageMain').style.marginRight = '3%';
					
					var marked = document.getElementsByClassName('bgcolor_overlay');
					if (marked.length > 0)
					{
						if (marked[0].previousSibling != null && marked[0].previousSibling.tagName == 'H3')
							marked[0].previousSibling.scrollIntoView(true);
						else
							marked[0].scrollIntoView(true);
					}");
					
					/*presentation.Control.Web.ExecuteJavascript(@"
							document.getElementsByClassName('top')[0].style.display = 'none';
							document.getElementsByClassName('navi')[0].style.display = 'none';
							document.getElementsByClassName('navi')[1].style.display = 'none';");*/
					
					
				}
			};

			presentation.Control.Web.LoadingFrame += (s, args) =>
			{
				// TODO
				this.urlTextBox.Text = args.Url.ToString();
			};
		}

		private void goButton_Click(object sender, RoutedEventArgs e)
		{
			isBible = false;
			string url = urlTextBox.Text;
			if (!url.Contains("://"))
				url = "http://" + url;

			presentation.Control.Web.LoadURL(new Uri(url));
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
			if (Controller.PresentationManager.CurrentPresentation != presentation)
				presentation.Close();
		}

		private void goBibleButton_Click(object sender, RoutedEventArgs e)
		{
			isBible = true;
			var bible = (BibleTranslation)translationComboBox.SelectedValue;
			string url;
			if (bible.Abbreviation != "")
				url = "http://m.bibleserver.com/text/" + bible.Abbreviation + "/" + bibleTextBox.Text;
			else
				url = "http://m.bibleserver.com/text/" + bibleTextBox.Text;

			presentation.Control.Web.LoadURL(new Uri(url));
			
			Controller.PresentationManager.CurrentPresentation = presentation;
		}

		private void bibleTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				goBibleButton_Click(this, new RoutedEventArgs());
			}
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
			presentation.Control.Web.GoBack();
		}

		private void buttonForward_Click(object sender, RoutedEventArgs e)
		{
			presentation.Control.Web.GoForward();
		}
	}
}
