using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using PowerpointViewerLib;
using WordsLive.Presentation;
using WordsLive.Slideshow.Resources;

namespace WordsLive.Slideshow.PowerpointViewer
{
	public class PowerpointViewerPresentation : SlideshowPresentationBase
	{
		private List<SlideThumbnail> thumbnails;
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
							Image = SlideshowPreviewProvider.ConvertBitmap(bmp),
							Title = String.Format(Resource.slideN, i) + " (" +
								(animations == 0 ? Resource.animations0 :
								(animations == 1 ? Resource.animations1 :
								String.Format(Resource.animationsPl, animations))) + ")"
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
		private FileInfo file;
		
		public void Init(FileInfo file)
		{
			this.file = file;
		}

		public override void Load()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(PerformLoad));
		}

		private void PerformLoad(object o)
		{
			try
			{
				doc = PowerpointViewerController.Open(this.file.FullName, new Rectangle(Area.WindowLocation.X, Area.WindowLocation.Y, Area.WindowSize.Width, Area.WindowSize.Height), openHidden: true, thumbnailWidth: 200);
				doc.Error += (sender, args) =>
				{
					if (!doc.HasLoaded)
					{
						Controller.Dispatcher.Invoke(new Action(() => OnLoaded(false)));
					}
					else
					{
						base.OnClosedExternally();
					}
				};

				doc.Loaded += (sender, args) =>
				{
					Controller.Dispatcher.Invoke(new Action(() =>
					{
						if (!isClosing)
						{
							WordsLive.Presentation.Wpf.AeroPeekHelper.RemoveFromAeroPeek(doc.WindowHandle);
							if (showOnLoaded)
							{
								doc.Move(Area.WindowLocation.X, Area.WindowLocation.Y);
								isShown = true;
							}
							OnLoaded(true);
							Controller.FocusMainWindow();
						}
						else
						{
							doc.Close();
						}
					}));
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
			catch (PowerpointViewerController.PowerpointViewerOpenException)
			{
				Controller.Dispatcher.Invoke(new Action(() =>
				{
					OnLoaded(false);
				}));
			}
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
			if (doc != null)
				doc.Close();

			base.Close();
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

		public override BitmapSource CaptureWindow(int width)
		{
			return SlideshowPreviewProvider.ConvertBitmap(doc.CaptureWindow(width));
		}

		
	}
}
