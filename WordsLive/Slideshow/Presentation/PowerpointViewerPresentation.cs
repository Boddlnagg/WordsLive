/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PowerpointViewerLib;
using WordsLive.Presentation;
using WordsLive.Presentation.Wpf;
using WordsLive.Resources;

namespace WordsLive.Slideshow.Presentation
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
							Image = Interop.ConvertBitmap(bmp),
							Title = String.Format(Resource.slideDescriptionN, i) + " (" +
								(animations == 0 ? Resource.slideAnimations0 :
								(animations == 1 ? Resource.slideAnimations1 :
								String.Format(Resource.slideAnimationsPl, animations))) + ")"
						});
						i++;
					}
				}
				return thumbnails;
			}
		}

		public override bool IsEndless
		{
			get
			{
				return doc.IsEndless;
			}
		}

		PowerpointViewerDocument doc;
		private bool showOnLoaded = false;
		private bool isShown = false;
		private bool hasShownHiddenWarning = false;

		public FileInfo File { get; set; }

		public override void Load()
		{
			// TODO: improve error handling
			Task.Factory.StartNew(PerformLoad).ContinueWith(t =>
				Controller.Dispatcher.Invoke(new Action(() => { base.OnLoaded(false); throw t.Exception.InnerException; })),
				TaskContinuationOptions.OnlyOnFaulted);
		}

		private void PerformLoad()
		{
			if (File == null)
				throw new InvalidOperationException("file has not been set");

			try
			{
				doc = PowerpointViewerController.Open(this.File.FullName, new Rectangle(Area.WindowLocation.X, Area.WindowLocation.Y, Area.WindowSize.Width, Area.WindowSize.Height), openHidden: true, thumbnailWidth: 200);
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
							WordsLive.Presentation.Wpf.Interop.RemoveFromAeroPeek(doc.WindowHandle);
							if (showOnLoaded)
							{
								doc.Move(Area.WindowLocation.X, Area.WindowLocation.Y);
								isShown = true;
							}
							OnLoaded(true);
							Controller.FocusMainWindow(true);
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

				doc.HiddenSlide += (sender, args) =>
				{
					Controller.Dispatcher.Invoke(new Action(() =>
					{
						if (!hasShownHiddenWarning)
						{
							MessageBox.Show(Resource.slideErrorMsgHiddenSlides, Resource.slideErrorMsgHiddenSlidesTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
							hasShownHiddenWarning = true;
						}
					}));
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

		protected override void LoadPreviewProvider()
		{
			Controller.Dispatcher.Invoke((Action)delegate
			{
				preview = new LiveWindowPreviewProvider(doc.WindowHandle);
			});
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
	}
}
