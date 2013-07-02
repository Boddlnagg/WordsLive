using WordsLive.Awesomium;

namespace WordsLive
{
	public class PdfPresentation : AwesomiumPresentation
	{
		public void GotoPage(int page)
		{
			this.Control.Web.ExecuteJavascript("gotoPage("+page.ToString()+");");
		}
	}
}
