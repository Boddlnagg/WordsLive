using System;
using System.Collections.Generic;
using Words.Presentation.Wpf;
using System.IO;

namespace Words.Images
{
	public class ImagesPresentation : WpfPresentation<ImagesControl>
	{
		private ImagesMedia media;
		private int index;

		public void Load(ImagesMedia media)
		{
			this.media = media;
			//images = new List<ImagesMedia.ImageInfo>(media.Photos);
			index = 0;
			Update();
		}

		public void GotoSlide(int index)
		{
			this.index = index;
			Update();
			
		}

		public void NextStep()
		{
			if (index < media.Images.Count - 1)
			{
				index++;
				Update();
			}
		}

		public void PreviousStep()
		{
			if (index > 0)
			{
				index--;
				Update();
			}
		}

		private void Update()
		{
			this.Control.ImageSource = media.Images[index].FullName;
			//if (index + 1 < images.Count)
			//	images[index + 1].PreCache();
		}

		public int SlideIndex
		{
			get
			{
				return index;
			}
		}
	}
}
