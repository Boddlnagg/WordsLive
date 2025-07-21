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
	public class XpsPresentation : WpfPresentation<DocumentViewer>, IDocumentPresentation
	{
		private DocumentPageScale pageScale = default;
		private FixedDocument document;
		private int pageNumber = 1;

		public XpsPresentation()
		{
			this.Control.Style = Application.Current.FindResource("reducedDocumentViewer") as Style;
			this.Control.ShowPageBorders = false;
			this.Control.HorizontalPageSpacing = 0;
			this.Control.VerticalPageSpacing = 0;
			this.Control.SizeChanged += (sender, args) => ApplyPageScale();
		}

		public void SetSourceDocument(XpsDocument doc)
		{
			this.pageScale = doc.PageScale;
			this.document = doc.Document.GetFixedDocumentSequence().References[0].GetDocument(false);
		}

		public void Load()
		{
			RenderCurrentPage();
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
			get { return this.document == null ? 0 : this.pageNumber; }
		}

		public void GoToPage(int page)
		{
			this.pageNumber = page;
			RenderCurrentPage();
		}

		public void PreviousPage()
		{
			if (!this.CanGoToPreviousPage)
				return;
			this.pageNumber--;
			RenderCurrentPage();
		}

		public void NextPage()
		{
			if (!this.CanGoToNextPage)
				return;
			this.pageNumber++;
			RenderCurrentPage();
		}

		public bool CanGoToPreviousPage
		{
			get
			{
				return this.pageNumber > 1;
			}
		}

		public bool CanGoToNextPage
		{
			get
			{
				return this.pageNumber < this.PageCount;
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

		private void RenderCurrentPage()
		{
			if (this.document.Pages.Count == 0)
				return;
			this.pageNumber = Math.Max(1, Math.Min(this.document.Pages.Count, this.pageNumber));

			/*
			 * We do not render the whole document, just the current page. This way ApplyPageScale
			 * will work properly (for each page individually, not only for the largest one of the whole
			 * document) and we will not ever see parts of other pages.
			 */
			var page = this.document.Pages[this.pageNumber - 1].GetPageRoot(false);

			/*
			 * A problem is that there is no easy way to copy individual pages for their rendering.
			 * Therefore we use CreateVisualBrushClone that renders the page into a new FixedPage.
			 * By rendering, we avoid other types of cloning and are sure that no referenced resources are lost.
			 */
			var clonedPage = CreateVisualBrushClone(page);

			var singlePageContent = new PageContent();
			((IAddChild)singlePageContent).AddChild(clonedPage);
			var singlePageDocument = new FixedDocument();
			singlePageDocument.Pages.Add(singlePageContent);

			this.Control.Document = singlePageDocument;
			ApplyPageScale();
		}

		private void ApplyPageScale()
		{
			if (pageScale == DocumentPageScale.FitToWidth)
				Control.FitToWidth();
			else
				Control.FitToMaxPagesAcross(1);
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

		public event EventHandler DocumentLoaded;

		protected void OnDocumentLoaded()
		{
			if (DocumentLoaded != null)
				DocumentLoaded(this, EventArgs.Empty);
		}
	}
}
