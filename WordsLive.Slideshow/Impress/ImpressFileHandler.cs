using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WordsLive.Core;
using WordsLive.Core.Data;

namespace WordsLive.Slideshow.Impress
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

		public override Media TryHandle(string path, IMediaDataProvider provider)
		{
			string ext = Path.GetExtension(path).ToLower();
			// prefer powerpoint viewer for ppts if available
			if ((ext == ".ppt" || ext == ".pptx") && PowerpointViewerLib.PowerpointViewerController.IsAvailable)
				return null;

			if (!IsAvailable)
				return null;

			return new ImpressMedia(path, provider, PresentationType);
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
				var asm = Assembly.LoadFrom("WordsLive.Slideshow.Impress.Bridge.dll");
				PresentationType = asm.GetType("WordsLive.Slideshow.Impress.Bridge.ImpressPresentation");
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
