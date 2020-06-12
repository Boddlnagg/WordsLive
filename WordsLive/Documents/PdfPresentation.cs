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
using WordsLive.Cef;
using CefSharp;

namespace WordsLive.Documents
{
	public class PdfPresentation : CefPresentation, IDocumentPresentation
	{
		//private JSObject bridge;
		private string uriKey;
		private DocumentPageScale pageScale;

		public PdfDocument Document { get; internal set; }

		public bool IsLoaded { get; private set; }

		private PdfPresentationBridge bridge;

		public void Load()
		{
			base.Load(true); // TODO: set to false
			int nr = 0;
			do
			{
				uriKey = "pdfdoc" + (nr++) + ".pdf";
			} while (UriMapDataSource.Instance.Exists(uriKey));


			UriMapDataSource.Instance.Add(uriKey, Document.Uri);

			this.bridge = new PdfPresentationBridge("asset://urimap/" + uriKey);
			this.Control.Web.JavascriptObjectRepository.UnRegisterAll();
			this.Control.Web.JavascriptObjectRepository.Register("bridge", bridge, true);
			bridge.CallbackLoaded += () => {
				this.Control.Dispatcher.BeginInvoke(new Action(() => {
					IsLoaded = true;
					OnDocumentLoaded();
				}));
			};

			//this.Control.Web.ConsoleMessage += (sender, args) => System.Windows.MessageBox.Show(args.Source + " (" + args.Line + "): " + args.Message);

			//(this.Control.Web as CefSharp.Wpf.ChromiumWebBrowser).IsBrowserInitializedChanged += (sender, args) => {
			//	int x = 0;
			//};
			this.Control.Web.Load("asset://WordsLive/pdf.html");
		}

		public void GoToPage(int page)
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.GetBrowser().MainFrame.ExecuteJavaScriptAsync("gotoPage("+page.ToString()+");");
		}

		public void NextPage()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.GetMainFrame().ExecuteJavaScriptAsync("nextPage()");
		}

		public void PreviousPage()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.GetMainFrame().ExecuteJavaScriptAsync("prevPage();");
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
						this.Control.Web.GetMainFrame().ExecuteJavaScriptAsync("fitToWidth()");
					else
						this.Control.Web.GetMainFrame().ExecuteJavaScriptAsync("wholePage()");
				}
			}
		}

		public int CurrentPage
		{
			get
			{
				if (!IsLoaded)
					throw new InvalidOperationException("Document not loaded yet.");

				var task = this.Control.Web.GetMainFrame().EvaluateScriptAsync("getCurrentPage()");
				task.Wait();
				if (!task.Result.Success)
					throw new InvalidOperationException("JS call did not succeed.");
				return (int)task.Result.Result;
			}
		}

		public int PageCount
		{
			get
			{
				if (!IsLoaded)
					throw new InvalidOperationException("Document not loaded yet.");

				var task = this.Control.Web.GetMainFrame().EvaluateScriptAsync("getPageCount()");
				task.Wait();
				if (!task.Result.Success)
					throw new InvalidOperationException("JS call did not succeed.");
				return (int)task.Result.Result;
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
