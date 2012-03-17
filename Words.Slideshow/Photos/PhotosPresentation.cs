﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using Words.Presentation;
using Words.Presentation.Wpf;

namespace Words.Slideshow.Photos
{
	public class PhotosPresentation : WpfPresentation<PhotosControl>, ISlideshowPresentation
	{
		private PhotosMedia media;
		private List<ImageSource> images;
		private int index;

		public void Init(PhotosMedia media)
		{
			this.media = media;
		}

		public void Load()
		{
			images = new List<ImageSource>(media.Images);
			index = 0;
			Update();
			OnLoaded();
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
			this.Control.ImageSource = images[index];
			OnSlideIndexChanged();
		}

		public IList<ImageSource> Thumbnails
		{
			get
			{
				return images;
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

		public event EventHandler Loaded;

		public event EventHandler SlideIndexChanged;
	}
}
