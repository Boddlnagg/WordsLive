using System;
using System.Windows;
using System.Windows.Controls;
using Words.Core;
using Words.Presentation.Wpf;

namespace Words
{
	[TargetMedia(typeof(WebSite))]
	public partial class WebSiteControlPanel : UserControl, IMediaControlPanel
	{
		private AwesomiumPresentation presentation;

		public WebSiteControlPanel()
		{
			InitializeComponent();
		}

		private Words.Core.WebSite website;

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
			website = media as Words.Core.WebSite;

			if (website == null)
				throw new ArgumentException("media must not be null and of type WebSite");

			this.urlTextBox.Text = website.Url;

			presentation = Controller.PresentationManager.CreatePresentation<AwesomiumPresentation>();
			presentation.Load(true);
		}

		private void goButton_Click(object sender, RoutedEventArgs e)
		{
			presentation.Control.Web.LoadURL(urlTextBox.Text);
			presentation.Control.Web.BeginLoading += (s, args) => this.urlTextBox.Text = args.Url;
			Controller.PresentationManager.CurrentPresentation = presentation;
		}

		public bool IsUpdatable
		{
			get { return false; }
		}

		public void Close()
		{
			if (Controller.PresentationManager.CurrentPresentation != presentation)
				presentation.Close();
		}
	}
}
