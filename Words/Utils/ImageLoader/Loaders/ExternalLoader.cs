using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Words.Utils.ImageLoader.Loaders
{
	class ExternalLoader : ILoader
	{
		#region ILoader Members

		public System.IO.Stream Load(object source)
		{
			var webClient = new WebClient();
			byte[] html = null;

			if (source is string)
				html = webClient.DownloadData(source as string);
			else if (source is Uri)
				html = webClient.DownloadData(source as Uri);

			if (html == null || html.Count() == 0) return null;

			return new MemoryStream(html);
		}

		#endregion
	}
}