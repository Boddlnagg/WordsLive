using System;
using System.Collections.Generic;
using Words.Presentation.Wpf;

namespace Words.Slideshow.Photos
{
	public class PhotosPresentation : WpfPresentation<PhotosControl>, ISlideshowPresentation
	{
		private PhotosMedia media;
		private List<PhotosMedia.PhotoInfo> images;
		private int index;

		public void Init(PhotosMedia media)
		{
			this.media = media;
		}

		public bool Load()
		{
			images = new List<PhotosMedia.PhotoInfo>(media.Photos);
			index = 0;
			Update();
			OnLoaded();
			return true;
		}

		public void GotoSlide(int index)
		{
			this.index = index;
			Update();
			
		}

		public void NextStep()
		{
			if (index < images.Count - 1)
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
			this.Control.ImageSource = images[index].LoadImage();
			if (index + 1 < images.Count)
				images[index + 1].PreCache();
			OnSlideIndexChanged();
		}

		public IList<SlideThumbnail> Thumbnails
		{
			get
			{
				return media.Thumbnails;
			}
		}

		public int SlideIndex
		{
			get
			{
				return index;
			}
		}

		protected void OnLoaded()
		{
			if (Loaded != null)
				Loaded(this, EventArgs.Empty);
		}

		protected void OnSlideIndexChanged()
		{
			if (SlideIndexChanged != null)
				SlideIndexChanged(this, EventArgs.Empty);
		}

		protected void OnClosedExternally() // this will never be called
		{
			if (ClosedExternally != null)
				ClosedExternally(this, EventArgs.Empty);
		}

		public event EventHandler ClosedExternally;

		public event EventHandler Loaded;

		public event EventHandler SlideIndexChanged;
	}
}
