using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WordsLive.Presentation.Wpf
{
	public class WindowThumbnail : FrameworkElement
	{
		public WindowThumbnail()
		{
			if (!Interop.IsDwmEnabled)
				throw new NotSupportedException("Creating a window thumbnail is not supported when DWM is not enabled.");

			this.LayoutUpdated += new EventHandler(Thumbnail_LayoutUpdated);
			this.Unloaded += new RoutedEventHandler(Thumbnail_Unloaded);
		}

		public static DependencyProperty SourceProperty;
		public static DependencyProperty ClientAreaOnlyProperty;

		static WindowThumbnail()
		{
			SourceProperty = DependencyProperty.Register(
				"Source",
				typeof(IntPtr),
				typeof(WindowThumbnail),
				new FrameworkPropertyMetadata(
					IntPtr.Zero,
					FrameworkPropertyMetadataOptions.AffectsMeasure,
					delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
					{
						((WindowThumbnail)obj).InitialiseThumbnail((IntPtr)args.NewValue);
					}));

			ClientAreaOnlyProperty = DependencyProperty.Register(
				"ClientAreaOnly",
				typeof(bool),
				typeof(WindowThumbnail),
				new FrameworkPropertyMetadata(
					false,
					FrameworkPropertyMetadataOptions.AffectsMeasure,
					delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
					{
						((WindowThumbnail)obj).UpdateThumbnail();
					}));

			OpacityProperty.OverrideMetadata(
				typeof(WindowThumbnail),
				new FrameworkPropertyMetadata(
					1.0,
					FrameworkPropertyMetadataOptions.Inherits,
					delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
					{
						((WindowThumbnail)obj).UpdateThumbnail();
					}));
		}

		public IntPtr Source
		{
			get { return (IntPtr)this.GetValue(SourceProperty); }
			set { this.SetValue(SourceProperty, value); }
		}

		public bool ClientAreaOnly
		{
			get { return (bool)this.GetValue(ClientAreaOnlyProperty); }
			set { this.SetValue(ClientAreaOnlyProperty, value); }
		}

		public new double Opacity
		{
			get { return (double)this.GetValue(OpacityProperty); }
			set { this.SetValue(OpacityProperty, value); }
		}

		private HwndSource target;
		private IntPtr thumb;

		private void InitialiseThumbnail(IntPtr source)
		{
			if (IntPtr.Zero != thumb)
			{
				// release the old thumbnail
				ReleaseThumbnail();
			}

			if (IntPtr.Zero != source)
			{
				// find our parent hwnd
				target = (HwndSource)HwndSource.FromVisual(this);

				// if we have one, we can attempt to register the thumbnail
				if (target != null && 0 == Interop.DwmRegisterThumbnail(target.Handle, source, out this.thumb))
				{
					Interop.ThumbnailProperties props = new Interop.ThumbnailProperties();
					props.Visible = false;
					props.SourceClientAreaOnly = this.ClientAreaOnly;
					props.Opacity = (byte)(255 * this.Opacity);
					props.Flags = Interop.ThumbnailFlags.Visible | Interop.ThumbnailFlags.SourceClientAreaOnly
						| Interop.ThumbnailFlags.Opacity;
					Interop.DwmUpdateThumbnailProperties(thumb, ref props);
				}
			}
		}

		private void ReleaseThumbnail()
		{
			Interop.DwmUnregisterThumbnail(thumb);
			this.thumb = IntPtr.Zero;
			this.target = null;
		}

		private void UpdateThumbnail()
		{
			if (IntPtr.Zero != thumb)
			{
				Interop.ThumbnailProperties props = new Interop.ThumbnailProperties();
				props.SourceClientAreaOnly = this.ClientAreaOnly;
				props.Opacity = (byte)(255 * this.Opacity);
				props.Flags = Interop.ThumbnailFlags.SourceClientAreaOnly | Interop.ThumbnailFlags.Opacity;
				Interop.DwmUpdateThumbnailProperties(thumb, ref props);
			}
		}

		private void Thumbnail_Unloaded(object sender, RoutedEventArgs e)
		{
			ReleaseThumbnail();
		}

		// this is where the magic happens
		private void Thumbnail_LayoutUpdated(object sender, EventArgs e)
		{
			if (IntPtr.Zero == thumb)
			{
				InitialiseThumbnail(this.Source);
			}

			if (IntPtr.Zero != thumb)
			{
				if (target.RootVisual == null || !target.RootVisual.IsAncestorOf(this))
				{
					//we are no longer in the visual tree
					ReleaseThumbnail();
					return;
				}

				GeneralTransform transform = TransformToAncestor(target.RootVisual);

				Point a = transform.Transform(new Point(0, 0));
				if (double.IsNaN(a.X))
				{
					this.InvalidateArrange();
				}
				else
				{
					Point b = transform.Transform(new Point(this.ActualWidth, this.ActualHeight));

					Interop.ThumbnailProperties props = new Interop.ThumbnailProperties();
					props.Visible = true;
					props.Destination = new Interop.Rect(
						(int)Math.Ceiling(a.X), (int)Math.Ceiling(a.Y),
						(int)Math.Ceiling(b.X), (int)Math.Ceiling(b.Y));
					props.Flags = Interop.ThumbnailFlags.Visible | Interop.ThumbnailFlags.RectDestination;
					Interop.DwmUpdateThumbnailProperties(thumb, ref props);
				}
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			Interop.Size size;
			Interop.DwmQueryThumbnailSourceSize(this.thumb, out size);

			double scale = 1;

			// our preferred size is the thumbnail source size
			// if less space is available, we scale appropriately
			if (size.Width > availableSize.Width)
				scale = availableSize.Width / size.Width;
			if (size.Height > availableSize.Height)
				scale = Math.Min(scale, availableSize.Height / size.Height);

			return new Size(size.Width * scale, size.Height * scale); ;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Interop.Size size;
			Interop.DwmQueryThumbnailSourceSize(this.thumb, out size);

			// scale to fit whatever size we were allocated
			double scale = finalSize.Width / size.Width;
			scale = Math.Min(scale, finalSize.Height / size.Height);

			return new Size(size.Width * scale, size.Height * scale);
		}
	}
}