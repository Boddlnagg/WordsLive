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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using WordsLive.Presentation.Wpf;

namespace WordsLive.Documents
{
	public class XpsPresentation : WpfPresentation<Grid>, IDocumentPresentation
	{
		private DocumentPageScale pageScale = default;
		private FixedDocument document;
		private DocumentViewer currentViewer;
		private int currentPageNumber = 1;
		private int crossfadeDuration = 500;

		public XpsPresentation()
		{
			this.Control.Background = Brushes.Black;
			this.Control.SizeChanged += (sender, args) => ApplyPageScale();
		}

		public void SetSourceDocument(XpsDocument doc)
		{
			this.pageScale = doc.PageScale;
			this.document = doc.Document.GetFixedDocumentSequence().References[0].GetDocument(false);
		}

		public void Load()
		{
			var newDocumentViewer = RenderPage(this.currentPageNumber);
			ShowPage(newDocumentViewer);
			IsLoaded = true;
			OnDocumentLoaded();
		}

		public bool IsLoaded { get; private set; }

		public int PageCount
		{
			get { return this.document == null ? 0 : this.document.Pages.Count; }
		}

		public int CurrentPage
		{
			get { return this.document == null ? 0 : this.currentPageNumber; }
		}

		public void GoToPage(int page)
		{
			var newDocumentViewer = RenderPage(page);
			this.currentPageNumber = page;
			AnimatePageTransition(newDocumentViewer);
		}

		public void PreviousPage()
		{
			if (!this.CanGoToPreviousPage)
				return;
			this.GoToPage(this.currentPageNumber - 1);
		}

		public void NextPage()
		{
			if (!this.CanGoToNextPage)
				return;
			this.GoToPage(this.currentPageNumber + 1);
		}

		public bool CanGoToPreviousPage
		{
			get
			{
				return this.currentPageNumber > 1;
			}
		}

		public bool CanGoToNextPage
		{
			get
			{
				return this.currentPageNumber < this.PageCount;
			}
		}

		public DocumentPageScale PageScale
		{
			get
			{
				return pageScale;
			}
			set
			{
				if (pageScale != value)
				{
					pageScale = value;
					ApplyPageScale();
				}
			}
		}

		private void ApplyPageScale()
		{
			if (currentViewer == null)
				return;
			if (pageScale == DocumentPageScale.FitToWidth)
				currentViewer.FitToWidth();
			else
				currentViewer.FitToMaxPagesAcross(1);
		}

		private DocumentViewer RenderPage(int pageNumber)
		{
			var documentViewer = new DocumentViewer
			{
				Style = Application.Current.FindResource("reducedDocumentViewer") as Style,
				ShowPageBorders = false,
				HorizontalPageSpacing = 0,
				VerticalPageSpacing = 0
			};

			if (this.document.Pages.Count == 0)
				return documentViewer;

			var page = this.document.Pages[Math.Max(1, Math.Min(this.document.Pages.Count, pageNumber)) - 1].GetPageRoot(false);

			/*
			 * A problem is that there is no straightforward way to render individual pages or just
			 * copy individual pages. Therefore we use CreateVisualBrushClone that renders the page
			 * into a new FixedPage. This way we avoid requiring a deep cloning and we are sure that
			 * no referenced resources are lost.
			 */
			var clonedPage = CreateVisualBrushClone(page);

			var singlePageContent = new PageContent();
			((IAddChild)singlePageContent).AddChild(clonedPage);
			var singlePageDocument = new FixedDocument();
			singlePageDocument.Pages.Add(singlePageContent);

			documentViewer.Document = singlePageDocument;
			if (pageScale == DocumentPageScale.FitToWidth)
				documentViewer.FitToWidth();
			else
				documentViewer.FitToMaxPagesAcross(1);

			return documentViewer;
		}

		private static FixedPage CreateVisualBrushClone(FixedPage original)
		{
			if (original == null)
				return null;

			var clone = new FixedPage
			{
				Width = original.Width,
				Height = original.Height
			};

			var rect = new System.Windows.Shapes.Rectangle
			{
				Width = original.Width,
				Height = original.Height,
				Fill = new VisualBrush(original)
			};

			clone.Children.Add(rect);
			return clone;
		}

		private void ShowPage(DocumentViewer newDocumentViewer)
		{
			this.Control.Children.Clear();
			this.Control.Children.Add(newDocumentViewer);
			this.currentViewer = newDocumentViewer;
		}

		private void AnimatePageTransition(DocumentViewer newDocumentViewer)
		{
			if (crossfadeDuration <= 0)
			{
				ShowPage(newDocumentViewer);
				return;
			}

			var oldDocumentViewer = this.currentViewer;

			newDocumentViewer.Opacity = 0.0;
			this.Control.Children.Add(newDocumentViewer);
			this.currentViewer = newDocumentViewer;

			var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(crossfadeDuration));
			fadeIn.Completed += (s, e) => this.Control.Children.Remove(oldDocumentViewer);
			newDocumentViewer.BeginAnimation(UIElement.OpacityProperty, fadeIn);
		}

		public event EventHandler DocumentLoaded;

		protected void OnDocumentLoaded()
		{
			if (DocumentLoaded != null)
				DocumentLoaded(this, EventArgs.Empty);
		}
	}
}
