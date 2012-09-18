using System.Windows.Controls;
using Words.Core;

namespace Words
{
	[TargetMedia(typeof(XpsDocument))]
	public partial class XpsDocumentControlPanel : UserControl, IMediaControlPanel
	{
		private XpsDocument doc;
		private XpsPresentation pres;

		public XpsDocumentControlPanel()
		{
			InitializeComponent();
		}

		public Control Control
		{
			get { return this; }
		}

		public Media Media
		{
			get { return doc; }
		}

		public void Init(Media media)
		{
			if (this.doc == null)
				pres = Controller.PresentationManager.CreatePresentation<XpsPresentation>();

			this.doc = (XpsDocument)media;
			pres.SetSourceDocument(doc);
			Controller.PresentationManager.CurrentPresentation = pres;
		}

		public bool IsUpdatable
		{
			get { return true; }
		}

		public ControlPanelLoadState LoadState
		{
			get { return ControlPanelLoadState.Loaded; }
		}

		public void Close()
		{
			
		}

		private void ZoomInButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.IncreaseZoom();
		}

		private void ZoomOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.DecreaseZoom();
		}

		private void FitToWidthButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.FitToWidth();
		}

		private void WholePageButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.FitToMaxPagesAcross(1);
		}

		private void MultiplePagesButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			int n;
			if (int.TryParse(MultiplePagesTextBox.Text, out n) && n >= 1 && n <= 32)
				pres.Control.FitToMaxPagesAcross(n);
		}

		private void ViewThumbnailsButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.ViewThumbnails();
		}

		private void PrevPageButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.PreviousPage();
		}

		private void NextPageButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.NextPage();
		}

		private void ActualSizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			pres.Control.Zoom = 100;
		}

		private void GoToPageButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			int page;
			if (int.TryParse(GoToPageTextBox.Text, out page) && page >= 1)
				pres.Control.GoToPage(page);
		}
	}
}
