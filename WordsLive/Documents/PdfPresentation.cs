using System;
using Awesomium.Core;
using WordsLive.Awesomium;

namespace WordsLive.Documents
{
	public class PdfPresentation : AwesomiumPresentation
	{
		JSObject bridge;
		string uriKey;

		public bool IsLoaded { get; private set; }

		public void Load(PdfDocument pdf)
		{
			base.Load(true); // TODO: set to false
			int nr = 0;
			do
			{
				uriKey = "pdfdoc" + (nr++) + ".pdf";
			} while (UriMapDataSource.Instance.Exists(uriKey));


			UriMapDataSource.Instance.Add(uriKey, pdf.Uri);
			bridge = this.Control.Web.CreateGlobalJavascriptObject("bridge");
			bridge["document"] = "asset://WordsLive/urimap/" + uriKey;
			bridge.Bind("loaded", false, (sender, args) => {
				IsLoaded = true;
				OnDocumentLoaded();
			});
			//presentation.Control.Web.ConsoleMessage += (sender, args) => MessageBox.Show(args.Message);
			this.Control.Web.LoadURL(new Uri("asset://WordsLive/pdf.html"));
		}

		public void GotoPage(int page)
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

		public void FitToWidth()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.ExecuteJavascript("fitToWidth()");
		}

		public void WholePage()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("Document not loaded yet.");

			this.Control.Web.ExecuteJavascript("wholePage()");
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
