using System;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Presentation.Wpf;

namespace WordsLive.Documents
{
	public class XpsPresentation : WpfPresentation<DocumentViewer>, IDocumentPresentation
	{
		public XpsPresentation()
		{
			this.Control.Style = Application.Current.FindResource("reducedDocumentViewer") as Style;
			this.Control.ShowPageBorders = false;
			this.Control.HorizontalPageSpacing = 0;
			this.Control.VerticalPageSpacing = 0;
		}
		public void SetSourceDocument(XpsDocument doc)
		{
			this.Control.Document = doc.Document.GetFixedDocumentSequence();
		}

		public void Load()
		{
			IsLoaded = true;
			OnDocumentLoaded();
		}

		public bool IsLoaded { get; private set; }

		public int PageCount
		{
			get { return Control.PageCount; }
		}

		public int CurrentPage
		{
			get { return Control.MasterPageNumber; }
		}

		public void GotoPage(int page)
		{
			Control.GoToPage(page);
		}

		public void PreviousPage()
		{
			Control.PreviousPage();
		}

		public void NextPage()
		{
			Control.NextPage();
		}

		public void FitToWidth()
		{
			Control.FitToWidth();
		}

		public void WholePage()
		{
			Control.FitToMaxPagesAcross(1);
		}

		public event EventHandler DocumentLoaded;

		protected void OnDocumentLoaded()
		{
			if (DocumentLoaded != null)
				DocumentLoaded(this, EventArgs.Empty);
		}
	}
}
