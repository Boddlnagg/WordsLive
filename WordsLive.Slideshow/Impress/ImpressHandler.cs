using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WordsLive.Core;

namespace WordsLive.Slideshow.Impress
{
	public class ImpressHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".odp", ".ppt", ".pptx" }; }
		}

		public override string Description
		{
			get { return "OpenDocument Presentation"; }
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
				string startupDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
				var asm = Assembly.LoadFrom(Path.Combine(startupDir, "WordsLive.Slideshow.Impress.Bridge.dll"));
				PresentationType = asm.GetType("WordsLive.Slideshow.Impress.Bridge.ImpressPresentation");
				if (PresentationType != null)
					isAvailable = true;
			}
			catch (FileNotFoundException)
			{
				throw new DllNotFoundException("WordsLive.Slideshow.Impress.Bridge.dll");
			}
			catch (ReflectionTypeLoadException)
			{
				isAvailable = false;
			}
		}

		public override int Test(Uri uri)
		{
			if (!IsAvailable)
				return -1;

			if (!CheckExtension(uri))
				return -1;

			string ext = uri.GetExtension();
			
			if ((ext == ".ppt" || ext == ".pptx"))
				return 50; // prefer powerpoint viewer for ppts if available

			return 100;
		}

		public override Media Handle(Uri uri, Dictionary<string, string> options)
		{
			return new ImpressMedia(uri, PresentationType);
		}
	}
}
