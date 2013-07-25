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
using WordsLive.Presentation.Wpf;

namespace WordsLive.Documents
{
	public class XpsPresentation : WpfPresentation<DocumentViewer>, IDocumentPresentation
	{
		private DocumentPageScale pageScale = DocumentPageScale.FitToWidth;

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
			this.Control.Document = doc.Document.GetFixedDocumentSequence();
			this.Control.FitToWidth();
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

		public void GoToPage(int page)
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

		public bool CanGoToPreviousPage
		{
			get
			{
				return Control.CanGoToPreviousPage;
			}
		}

		public bool CanGoToNextPage
		{
			get
			{
				return Control.CanGoToNextPage;
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
			if (pageScale == DocumentPageScale.FitToWidth)
				Control.FitToWidth();
			else
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
