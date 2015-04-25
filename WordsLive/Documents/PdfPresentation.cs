/*
 * WordsLive - worship projection software
 * Copyright (c) 2015 Patrick Reisert
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
using Awesomium.Core;
using WordsLive.Awesomium;

namespace WordsLive.Documents
{
	public class PdfPresentation : AwesomiumPresentation, IDocumentPresentation
	{
		private JSObject bridge;
		private string uriKey;
		private DocumentPageScale pageScale;

		public PdfDocument Document { get; internal set; }

		public bool IsLoaded { get; private set; }

		public void Load()
		{
			base.Load(true); // TODO: set to false
			int nr = 0;
			do
			{
				uriKey = "pdfdoc" + (nr++) + ".pdf";
			} while (UriMapDataSource.Instance.Exists(uriKey));


			UriMapDataSource.Instance.Add(uriKey, Document.Uri);
			bridge = this.Control.Web.CreateGlobalJavascriptObject("bridge");
			bridge["document"] = "asset://WordsLive/urimap/" + uriKey;
			bridge.BindAsync(loaded); // must be a named method, no lambda
			//this.Control.Web.ConsoleMessage += (sender, args) => System.Windows.MessageBox.Show(args.Message);
			this.Control.Web.Source = new Uri("asset://WordsLive/pdf.html");
		}

		private void loaded(object sender, JavascriptMethodEventArgs e)
		{
			IsLoaded = true;
			OnDocumentLoaded();
		}

		public void GoToPage(int page)
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.ExecuteJavascript("gotoPage("+page.ToString()+");");
		}

		public void NextPage()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.ExecuteJavascript("nextPage();");
		}

		public void PreviousPage()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.ExecuteJavascript("prevPage();");
		}

		public bool CanGoToPreviousPage
		{
			get
			{
				if (!IsLoaded)
					throw new InvalidOperationException("Document not loaded yet.");

				return CurrentPage > 1;
			}
		}

		public bool CanGoToNextPage
		{
			get
			{
				if (!IsLoaded)
					throw new InvalidOperationException("Document not loaded yet.");

				return CurrentPage < PageCount;
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
				pageScale = value;

				if (IsLoaded)
				{
					if (pageScale == DocumentPageScale.FitToWidth)
						this.Control.Web.ExecuteJavascript("fitToWidth()");
					else
						this.Control.Web.ExecuteJavascript("wholePage()");
				}
			}
		}

		public int CurrentPage
		{
			get
			{
				if (!IsLoaded)
					throw new InvalidOperationException("Document not loaded yet.");

				return (int)this.Control.Web.ExecuteJavascriptWithResult("getCurrentPage()");
			}
		}

		public int PageCount
		{
			get
			{
				if (!IsLoaded)
					throw new InvalidOperationException("Document not loaded yet.");

				return (int)this.Control.Web.ExecuteJavascriptWithResult("getPageCount()");
			}
		}

		public override void Close()
		{
			base.Close();

			UriMapDataSource.Instance.Remove(uriKey);
		}

		public event EventHandler DocumentLoaded;

		protected void OnDocumentLoaded()
		{
			if (DocumentLoaded != null)
				DocumentLoaded(this, EventArgs.Empty);
		}
	}
}
