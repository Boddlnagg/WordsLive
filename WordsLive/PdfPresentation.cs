using System;
using Awesomium.Core;
using WordsLive.Awesomium;

namespace WordsLive
{
	public class PdfPresentation : AwesomiumPresentation
	{
		JSObject bridge;
		string uriKey;

		public void Load(PdfMedia pdf)
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
			//presentation.Control.Web.ConsoleMessage += (sender, args) => MessageBox.Show(args.Message);
			this.Control.Web.LoadURL(new Uri("asset://WordsLive/pdf.html"));
		}

		public void GotoPage(int page)
		{
			this.Control.Web.ExecuteJavascript("gotoPage("+page.ToString()+");");
		}

		public void NextPage()
		{
			this.Control.Web.ExecuteJavascript("nextPage();");
		}

		public void PreviousPage()
		{
			this.Control.Web.ExecuteJavascript("prevPage();");
		}

		public void FitToWidth()
		{
			this.Control.Web.ExecuteJavascript("fitToWidth()");
		}

		public void WholePage()
		{
			this.Control.Web.ExecuteJavascript("wholePage()");
		}

		public override void Close()
		{
			base.Close();

			UriMapDataSource.Instance.Remove(uriKey);
		}
	}
}
