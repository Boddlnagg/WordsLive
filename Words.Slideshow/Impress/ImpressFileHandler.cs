using System.Collections.Generic;
using System.IO;
using Words.Core;

namespace Words.Slideshow.Impress
{
	public class ImpressFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".odp", ".ppt", ".pptx" }; }
		}

		public override string Description
		{
			get { return "OpenDocument Presentation"; }
		}

		public override Media TryHandle(FileInfo file)
		{
			// prefer powerpoint viewer for ppts if available
			if ((file.Extension.ToLower() == ".ppt" || file.Extension.ToLower() == ".pptx") && PowerpointViewerLib.PowerpointViewerController.IsAvailable)
				return null;

			// TODO (Slideshow.Impress): check if impress is available

			var media = new ImpressMedia();
			media.LoadMetadata(file.FullName);
			return media;
		}
	}
}
