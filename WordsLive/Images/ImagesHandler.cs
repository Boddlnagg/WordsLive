using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WordsLive.Core;
using WordsLive.Resources;

namespace WordsLive.Images
{
	public class ImagesHandler : MediaTypeHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return ImagesMedia.ImageExtensions.Concat(ImagesMedia.SlideshowExtensions); }
		}

		public override string Description
		{
			get { return "Diashows & Bilder"; } // TODO: localize
		}

		public override int Test(Uri uri)
		{
			return CheckExtension(uri)  ? 100 : -1;
		}

		public override Media Handle(Uri uri)
		{
			return new ImagesMedia(uri);
		}

		public override int TestMultiple(IEnumerable<Uri> uris)
		{
			return uris.All(u => ImagesMedia.IsValidImageUri(u)) ? 100 : -1;
		}

		public override IEnumerable<Media> HandleMultiple(IEnumerable<Uri> uris)
		{
			Controller.FocusMainWindow();

			var res = MessageBox.Show(Resource.imgMsgMultipleCreateSlideshow, Resource.imgMsgMultipleCreateSlideshowTitle, MessageBoxButton.YesNoCancel);
			if (res == MessageBoxResult.Yes)
			{
				Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
				dlg.DefaultExt = ".show";
				dlg.Filter = "Diashow|*.show"; // TODO: localize

				if (dlg.ShowDialog() == true)
				{
					var media = new ImagesMedia(new Uri(dlg.FileName));
					media.CreateSlideshow(uris);
					return new Media[] { media };
				}
				else
				{
					return new Media[] { };
				}
			}
			else if (res == MessageBoxResult.No)
			{
				return uris.Select(u => Handle(u)); // add them one by one
			}
			else // cancelled -> add nothing
			{
				return new Media[] { };
			}
			
		}
	}
}
