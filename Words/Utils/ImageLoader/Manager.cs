using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Words.Utils.ImageLoader.Loaders;

namespace Words.Utils.ImageLoader
{
	internal sealed class Manager
	{
		internal class LoadImageRequest
		{
			public bool IsCanceled { get; set; }
			public object Source { get; set; }
			public Stream Stream { get; set; }
			public Image Image { get; set; }
		}

		#region Properties
		private Thread _loaderThreadForThumbnails;
		private Thread _loaderThreadForNormalSize;

		private Dictionary<Image, LoadImageRequest> _imagesLastRunningTask = new Dictionary<Image, LoadImageRequest>();

		private Queue<LoadImageRequest> _loadThumbnailQueue = new Queue<LoadImageRequest>();
		private Stack<LoadImageRequest> _loadNormalStack = new Stack<LoadImageRequest>();

		private AutoResetEvent _loaderThreadThumbnailEvent = new AutoResetEvent(false);
		private AutoResetEvent _loaderThreadNormalSizeEvent = new AutoResetEvent(false);

		private ImageSource _loadingImage = null;
		private ImageSource _errorThumbnail = null;
		private TransformGroup _loadingAnimationTransform = null;
		#endregion

		#region Singleton Implementation
		private static readonly Manager instance = new Manager();

		private Manager()
		{
			#region Creates Loading Threads
			_loaderThreadForThumbnails = new Thread(new ThreadStart(LoaderThreadThumbnails));
			_loaderThreadForThumbnails.IsBackground = true;  // otherwise, the app won't quit with the UI...
			_loaderThreadForThumbnails.Priority = ThreadPriority.BelowNormal;
			_loaderThreadForThumbnails.Start();

			_loaderThreadForNormalSize = new Thread(new ThreadStart(LoaderThreadNormalSize));
			_loaderThreadForNormalSize.IsBackground = true;  // otherwise, the app won't quit with the UI...
			_loaderThreadForNormalSize.Priority = ThreadPriority.BelowNormal;
			_loaderThreadForNormalSize.Start();
			#endregion

			#region Loading Images from Resources
			ResourceDictionary resourceDictionary = new ResourceDictionary();
			resourceDictionary.Source = new Uri("Words;component/Utils/ImageLoader/Resources.xaml", UriKind.Relative);
			_loadingImage = resourceDictionary["ImageLoading"] as DrawingImage;
			_loadingImage.Freeze();
			_errorThumbnail = resourceDictionary["ImageError"] as DrawingImage;
			_errorThumbnail.Freeze();
			#endregion

			# region Create Loading Animation
			ScaleTransform scaleTransform = new ScaleTransform(0.5, 0.5);
			SkewTransform skewTransform = new SkewTransform(0, 0);
			RotateTransform rotateTransform = new RotateTransform(0);
			TranslateTransform translateTransform = new TranslateTransform(0, 0);

			TransformGroup group = new TransformGroup();
			group.Children.Add(scaleTransform);
			group.Children.Add(skewTransform);
			group.Children.Add(rotateTransform);
			group.Children.Add(translateTransform);

			DoubleAnimation doubleAnimation = new DoubleAnimation(0, 359, new TimeSpan(0, 0, 0, 1));
			doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

			rotateTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);

			_loadingAnimationTransform = group;
			#endregion
		}

		public static Manager Instance
		{
			get
			{
				return instance;
			}
		}

		public ImageSource LoadingImage
		{
			get
			{
				return _loadingImage;
			}
			set
			{
				_loadingImage = value;
			}
		}

		public ImageSource ErrorImage
		{
			get
			{
				return _errorThumbnail;
			}
			set
			{
				_errorThumbnail = value;
			}

		}
		#endregion

		#region Public Methods
		public void LoadImage(object source, Image image)
		{
			LoadImageRequest loadTask = new LoadImageRequest() { Image = image, Source = source };

			// Begin Loading
			BeginLoading(image, loadTask);

			lock (_loadThumbnailQueue)
			{
				_loadThumbnailQueue.Enqueue(loadTask);                
			}

			_loaderThreadThumbnailEvent.Set();
		}

		internal void OnImageUnloaded(object sender, RoutedEventArgs args)
		{
			var img = (Image)sender;

			lock (_loadThumbnailQueue)
			{
				foreach (var i in _loadThumbnailQueue)
				{
					if (i.Image == img)
					{
						i.IsCanceled = true;
						break;
					}
				}
			}
		}
		#endregion

		#region Private Methods
		private void BeginLoading(Image image, LoadImageRequest loadTask)
		{
			lock (_imagesLastRunningTask)
			{
				if (_imagesLastRunningTask.ContainsKey(image))
				{ // Cancel previous loading...
					_imagesLastRunningTask[image].IsCanceled = true;
					_imagesLastRunningTask[image] = loadTask;
				}
				else
				{
					_imagesLastRunningTask.Add(image, loadTask);
				}
			}

			image.Dispatcher.BeginInvoke(new ThreadStart(delegate
			{
				// Set IsLoading Pty
				Loader.SetIsLoading(image, true);

				if (image.RenderTransform == MatrixTransform.Identity) // Don't apply loading animation if image already has transform...
				{
					// Manage Waiting Image Parameter
					if (Loader.GetDisplayWaitingAnimationDuringLoading(image))
					{
						image.Source = _loadingImage;
						image.RenderTransformOrigin = new Point(0.5, 0.5);
						image.RenderTransform = _loadingAnimationTransform;
					}
				}
			}));
		}

		private void EndLoading(Image image, ImageSource imageSource, LoadImageRequest loadTask, bool markAsFinished)
		{
			lock (_imagesLastRunningTask)
			{
				if (_imagesLastRunningTask.ContainsKey(image))
				{ 
					if (_imagesLastRunningTask[image].Source != loadTask.Source)
						return; // if the last launched task for this image is not this one, abort it!

					if (markAsFinished)
						_imagesLastRunningTask.Remove(image);
				}
				else
				{
					/* ERROR! */
					System.Diagnostics.Debug.WriteLine("EndLoading() - unexpected condition: there is no running task for this image!");
				}

				image.Dispatcher.BeginInvoke(new ThreadStart(delegate
				{
					if (image.RenderTransform == _loadingAnimationTransform)
					{
						image.RenderTransform = MatrixTransform.Identity;
					}

					if (Loader.GetErrorDetected(image) && Loader.GetDisplayErrorThumbnailOnError(image))
					{
						imageSource = _errorThumbnail;
					}

					image.Source = imageSource;

					if (markAsFinished)
					{
						// Set IsLoading Pty
						Loader.SetIsLoading(image, false);
					}
				}));
			}
		}

		private ImageSource GetBitmapSource(LoadImageRequest loadTask, DisplayOptions loadType)
		{
			Image image = loadTask.Image;
			object source = loadTask.Source;
			ImageSource imageSource = null;

			if (source != null)
			{
				Stream imageStream = null;

				SourceType sourceType = SourceType.LocalDisk;

				image.Dispatcher.Invoke(new ThreadStart(delegate
				{
					sourceType = Loader.GetSourceType(image);
				}));

				try
				{
					if (loadType != DisplayOptions.VideoPreview)
					{
						if (loadTask.Stream == null)
						{
							ILoader loader = LoaderFactory.CreateLoader(sourceType);
							imageStream = loader.Load(source);
							loadTask.Stream = imageStream;
						}
						else
						{
							imageStream = new MemoryStream();
							loadTask.Stream.Position = 0;
							loadTask.Stream.CopyTo(imageStream);
							imageStream.Position = 0;
						}
					}
					else if (sourceType == SourceType.ZipFile)
					{
						throw new InvalidOperationException("Can't load video preview from zip file.");
					}
				}
				catch (Exception) { }

				if (imageStream != null || loadType == DisplayOptions.VideoPreview)
				{
					try
					{
						if (loadType == DisplayOptions.Preview)
						{
							BitmapFrame bitmapFrame = BitmapFrame.Create(imageStream);
							int rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);

							if (bitmapFrame.Thumbnail != null)
							{
								BitmapSource src = bitmapFrame.Thumbnail;
								// crop black bars if necessary
								double ratio = (double)bitmapFrame.PixelWidth / bitmapFrame.PixelHeight;
								double thumbRatio = (double)src.PixelWidth / src.PixelHeight;
								if (Math.Abs(ratio - thumbRatio) >= 0.01)
								{
									if (ratio > thumbRatio) // crop top/bottom
									{
										int newHeight = (int)(src.PixelWidth / ratio);
										int top = (src.PixelHeight - newHeight) / 2;
										src = new CroppedBitmap(src, new Int32Rect(0, top, src.PixelWidth, newHeight));
									}
									else // crop left/right
									{
										int newWidth = (int)(src.PixelHeight * ratio);
										int left = (src.PixelWidth - newWidth) / 2;
										src = new CroppedBitmap(src, new Int32Rect(left, 0, newWidth, src.PixelHeight));
									}
								}

								TransformedBitmap thumbnail = new TransformedBitmap();
								thumbnail.BeginInit();
								thumbnail.Source = src;
								TransformGroup transformGroup = new TransformGroup();
								// rotate according to metadata
								transformGroup.Children.Add(new RotateTransform(rotation));
								thumbnail.Transform = transformGroup;
								thumbnail.EndInit();
								imageSource = thumbnail;
							}
							else // Preview it is not embedded into the file
							{
								// we'll make a thumbnail image then ... (too bad as the pre-created one is FAST!)
								TransformedBitmap thumbnail = new TransformedBitmap();
								thumbnail.BeginInit();
								thumbnail.Source = bitmapFrame as BitmapSource;

								// we'll make a reasonable sized thumnbail with a height of 240
								int pixelH = bitmapFrame.PixelHeight;
								int pixelW = bitmapFrame.PixelWidth;
								int decodeH = 240;
								int decodeW = (pixelW * decodeH) / pixelH;
								double scaleX = decodeW / (double)pixelW;
								double scaleY = decodeH / (double)pixelH;
								TransformGroup transformGroup = new TransformGroup();
								transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
								transformGroup.Children.Add(new RotateTransform(rotation));
								thumbnail.Transform = transformGroup;
								thumbnail.EndInit();

								// this will disconnect the stream from the image completely ...
								WriteableBitmap writable = new WriteableBitmap(thumbnail);
								writable.Freeze();
								imageSource = writable;
							}
						}
						else if (loadType == DisplayOptions.FullResolution)
						{
							BitmapFrame bitmapFrame = BitmapFrame.Create(imageStream);
							int rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);

							TransformedBitmap bitmapImage = new TransformedBitmap();
							bitmapImage.BeginInit();
							bitmapImage.Source = bitmapFrame as BitmapSource;
							TransformGroup transformGroup = new TransformGroup();
							transformGroup.Children.Add(new RotateTransform(rotation));
							bitmapImage.Transform = transformGroup;
							bitmapImage.EndInit();

							bitmapImage.Freeze();

							imageSource = bitmapImage;
						}
						else if (loadType == DisplayOptions.VideoPreview)
						{
							var player = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };

							Uri uri;
							if (loadTask.Source is string)
								uri = new Uri(loadTask.Source as string);
							else if (loadTask.Source is Uri)
								uri = loadTask.Source as Uri;
							else
								throw new InvalidOperationException();

							player.Open(uri);
							player.Pause();
							player.Position = new TimeSpan(0, 0, 20); // go to 20 seconds (if the video is shorter, a black image will be captured)
							
							Thread.Sleep(1000);

							var pixelW = player.NaturalVideoWidth;
							var pixelH = player.NaturalVideoHeight;

							int decodeH = 240;
							int decodeW = (pixelW * decodeH) / pixelH;

							var rtb = new RenderTargetBitmap(decodeW, decodeH, 96, 96, PixelFormats.Pbgra32);
							DrawingVisual dv = new DrawingVisual();

							using (DrawingContext dc = dv.RenderOpen())
								dc.DrawVideo(player, new Rect(0, 0, decodeW, decodeH));

							rtb.Render(dv);
							imageSource = (ImageSource)BitmapFrame.Create(rtb).GetCurrentValueAsFrozen();
							player.Close();
						}
					}
					catch (Exception) { }
				}

				if (imageSource == null)
				{
					image.Dispatcher.BeginInvoke(new ThreadStart(delegate
					{
						Loader.SetErrorDetected(image, true);
					}));
				}
				else
				{
					imageSource.Freeze();

					image.Dispatcher.BeginInvoke(new ThreadStart(delegate
					{
						Loader.SetErrorDetected(image, false);
					}));
				}
			}
			else
			{
				image.Dispatcher.BeginInvoke(new ThreadStart(delegate
				{
					Loader.SetErrorDetected(image, false);
				}));
			}

			return imageSource;
		}

		private int GetRotation(BitmapMetadata metadata)
		{
			int rotation = 0;

			if ((metadata != null) && metadata.ContainsQuery("System.Photo.Orientation"))
			{
				object o = metadata.GetQuery("System.Photo.Orientation");

				if (o != null)
				{
					//refer to http://www.impulseadventure.com/photo/exif-orientation.html for details on orientation values
					switch ((ushort)o)
					{
						case 6:
							rotation = 90;
							break;
						case 3:
							rotation = 180;
							break;
						case 8:
							rotation = 270;
							break;
					}
				}
			}

			return rotation;
		}

		private void LoaderThreadThumbnails()
		{
			do
			{
				_loaderThreadThumbnailEvent.WaitOne();

				LoadImageRequest loadTask = null;

				do
				{

					lock (_loadThumbnailQueue)
					{
						loadTask = _loadThumbnailQueue.Count > 0 ? _loadThumbnailQueue.Dequeue() : null;
					}

					if (loadTask != null && !loadTask.IsCanceled)
					{
						DisplayOptions displayOption = DisplayOptions.Preview;

						loadTask.Image.Dispatcher.Invoke(new ThreadStart(delegate
						{
							displayOption = Loader.GetDisplayOption(loadTask.Image);
						}));

						ImageSource bitmapSource;

						if (displayOption == DisplayOptions.VideoPreview)
							bitmapSource = GetBitmapSource(loadTask, DisplayOptions.VideoPreview);
						else
							bitmapSource = GetBitmapSource(loadTask, DisplayOptions.Preview);

						if (displayOption == DisplayOptions.Preview || displayOption == DisplayOptions.VideoPreview)
						{
							EndLoading(loadTask.Image, bitmapSource, loadTask, true);
						}
						else if (displayOption == DisplayOptions.FullResolution)
						{
							EndLoading(loadTask.Image, bitmapSource, loadTask, false);

							lock (_loadNormalStack)
							{
								_loadNormalStack.Push(loadTask);
							}

							_loaderThreadNormalSizeEvent.Set();

						}
					}

				} while (loadTask != null);

			} while (true);
		}

		private void LoaderThreadNormalSize()
		{
			do
			{
				_loaderThreadNormalSizeEvent.WaitOne();

				LoadImageRequest loadTask = null;

				do
				{

					lock (_loadNormalStack)
					{
						loadTask = _loadNormalStack.Count > 0 ? _loadNormalStack.Pop() : null;
					}

					if (loadTask != null && !loadTask.IsCanceled)
					{
						ImageSource bitmapSource = GetBitmapSource(loadTask, DisplayOptions.FullResolution);
						EndLoading(loadTask.Image, bitmapSource, loadTask, true);
					}

				} while (loadTask != null);

			} while (true);
		}
		#endregion
	}
}
