using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Words.Core;

namespace Words.Slideshow.Photos
{
	public class PhotosFileHandler : MediaFileHandler
	{
		public override IEnumerable<string> Extensions
		{
			get { return new string[] { ".show", ".jpg", ".jpeg", ".png" }; } // TODO: add more
		}

		public override string Description
		{
			get { return "Diashows & Bilder"; }
		}

		public override Media TryHandle(FileInfo file)
		{
			var media = new PhotosMedia();
			media.LoadMetadata(file.FullName);
			return media;
		}

		public override Media[] TryHandleMultiple(FileInfo[] files)
		{
			if (files.All(f => f.Extension.ToLower() != ".show"))
			{
				Controller.FocusMainWindow();
				var res = MessageBox.Show("Sie haben mehrere Bilddateien ausgewählt. Wollen Sie diese als Diashow hinzufügen? Bei „Ja“ müssen Sie als nächstes auswählen, wohin die Diashow gespeichert werden soll. Bei „Nein“ werden die Bilder einzeln hinzugefügt.", "Slideshow erstellen?", MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Yes)
				{
					Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
					dlg.DefaultExt = ".show";
					dlg.Filter = "Diashow|*.show";

					if (dlg.ShowDialog() == true)
					{
						// TODO: move writing the actual file to PhotosMedia
						using (StreamWriter writer = new StreamWriter(dlg.FileName))
						{
							foreach (var f in files)
								writer.WriteLine(f.FullName);
						}
						var media = new PhotosMedia();
						media.LoadMetadata(dlg.FileName);
						return new Media[] { media };
					}
					else
					{
						return new Media[] { };
					}
				}
				if (res == MessageBoxResult.Cancel)
				{
					return new Media[] { };
				}
				else if (res == MessageBoxResult.No)
				{
					return null;
				}
			}

			return null;
		}
	}
}
