﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using PowerpointViewerLib;
using Words.Presentation;

namespace Words.Slideshow.Powerpoint
{
	public class PowerpointViewerPresentation : SlideshowPresentationBase
	{
		private List<SlideThumbnail> thumbnails;
		private PowerpointViewerMedia ppt;
		bool isClosing = false;

		public override IList<SlideThumbnail> Thumbnails
		{
			get
			{
				if (thumbnails == null && doc.HasLoaded)
				{
					thumbnails = new List<SlideThumbnail>();
					int i = 1;
					foreach (Bitmap bmp in doc.Thumbnails)
					{
						int animations = doc.GetSlideStepCount(i - 1) - 1;
						thumbnails.Add(new SlideThumbnail
						{
							Image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height)),
							Title = String.Format("Folie {0} ({1})", i, animations == 0 ? "keine Animation" : (animations == 1 ? " 1 Animationsschritt" : animations + " Animationsschritte"))
						});
						i++;
					}
				}
				return thumbnails;
			}
		}

		PowerpointViewerDocument doc;
		private bool showOnLoaded = false;
		private bool isShown = false;
		
		public void Init(PowerpointViewerMedia ppt)
		{
			this.ppt = ppt;
		}

		public override void Load()
		{
			doc = PowerpointViewerController.Open(ppt.File, new Rectangle(Area.WindowLocation.X, Area.WindowLocation.Y, Area.WindowSize.Width, Area.WindowSize.Height), openHidden: true);
			
			doc.Loaded += (sender, args) =>
			{
				Words.Presentation.Wpf.AeroPeekHelper.RemoveFromAeroPeek(doc.WindowHandle);
				if (showOnLoaded)
				{
					doc.Move(Area.WindowLocation.X, Area.WindowLocation.Y);
					isShown = true;
				}

				base.OnLoaded();
			};

			doc.Closed += (sender, args) =>
			{
				if (!isClosing)
					base.OnClosedExternally();
			};

			doc.SlideChanged += (sender, args) =>
			{
				base.OnSlideIndexChanged();
			};
			LoadPreviewProvider();
		}

		public override void Show()
		{
			if (doc.HasLoaded)
			{
				doc.Move(Area.WindowLocation.X, Area.WindowLocation.Y);
				isShown = true;
			}
			else
			{
				showOnLoaded = true;
			}
		}

		public override void Close()
		{
			isClosing = true;
			doc.Close();
		}

		public override void Hide()
		{
			if (doc.HasLoaded)
			{
				doc.Hide();
				isShown = false;
			}
			else
			{
				showOnLoaded = false;
			}
		}

		public override void Init(PresentationArea area)
		{
			base.Init(area);

			this.Area.WindowLocationChanged += (sender, args) =>
			{
				if (isShown)
				{
					doc.Move(Area.WindowLocation.X, Area.WindowLocation.Y);
				}
			};
		}

		public override int SlideIndex
		{
			get
			{
				return doc.CurrentSlide;
			}
		}

		public override void GotoSlide(int index)
		{
			doc.GotoSlide(index);
		}

		public override void NextStep()
		{
			doc.NextStep();
		}

		public override void PreviousStep()
		{
			doc.PrevStep();
		}
	}
}
