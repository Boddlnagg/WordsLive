using System.Windows;
using System.Windows.Controls;
using WordsLive.Presentation.Wpf;

namespace WordsLive.Documents
{
	public class XpsPresentation : WpfPresentation<DocumentViewer>
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
	}
}
