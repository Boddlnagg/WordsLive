using System;
using System.Collections.Generic;
using Words.Presentation.Wpf;
using System.IO;

namespace Words.Images
{
	public class ImagesPresentation : WpfPresentation<ImagesControl>
	{
		public ImagesPresentation()
		{
			this.Control.LoadingFinished += (sender, args) => OnLoadingFinished();
		}

		public void ShowImage(FileInfo file)
		{
			this.Control.ImageSource = file.FullName;
		}

		public event EventHandler LoadingFinished;

		protected virtual void OnLoadingFinished()
		{
			if (LoadingFinished != null)
				LoadingFinished(this, EventArgs.Empty);
		}
	}
}
