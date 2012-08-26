using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

			if (!IsAvailable)
				return null;

			var media = new ImpressMedia(PresentationType);
			media.LoadMetadata(file.FullName);
			return media;
		}

		private bool? isAvailable = null;

		public bool IsAvailable
		{
			get
			{
				if (!isAvailable.HasValue)
					TryLoadBridge();

				return isAvailable.Value;
			}
		}

		internal Type PresentationType { get; private set; }

		private void TryLoadBridge()
		{
			try
			{
				var asm = Assembly.LoadFrom("Words.Slideshow.Impress.Bridge.dll");
				PresentationType = asm.GetType("Words.Slideshow.Impress.Bridge.ImpressPresentation");
				if (PresentationType != null)
					isAvailable = true;
			}
			catch (ReflectionTypeLoadException)
			{
				isAvailable = false;
			}
		}
	}
}
