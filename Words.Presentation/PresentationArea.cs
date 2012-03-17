using System;
using System.Drawing;
using System.Windows.Forms;

namespace Words.Presentation
{
	public class PresentationArea
	{
		/// <summary>
		/// Initializes a new instance of the PresentationArea class.
		/// This uses the first available screen and starts in fullscreen mode.
		/// </summary>
		public PresentationArea()
		{
			this.Screen = Screen.AllScreens[0];
			this.Size = new Size(800,600);
			this.Offset = new Point(0, 0);
			this.Fullscreen = true;
		}
		
		/// <summary>
		/// Indicates that the WindowLocation has changed and a corresponding window should be moved.
		/// </summary>
		public event EventHandler WindowLocationChanged;
		
		protected virtual void OnWindowLocationChanged()
		{
			if (WindowLocationChanged != null) {
				WindowLocationChanged(this, EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// Indicates that the WindowSize has changed and a corresponding window should be resized.
		/// </summary>
		public event EventHandler WindowSizeChanged;
		
		protected virtual void OnWindowSizeChanged()
		{
			if (WindowSizeChanged != null) {
				WindowSizeChanged(this, EventArgs.Empty);
			}
		}
		
		private Screen screen;
		
		/// <summary>
		/// Gets or sets the current screen the PresentationArea is on.
		/// </summary>
		public Screen Screen
		{
			get
			{
				return screen;
			}
			set
			{
				bool sizeChanged = false;
				if(Fullscreen && screen.Bounds.Size != value.Bounds.Size)
					sizeChanged = true;
				
				screen = value;
				
				this.OnWindowLocationChanged();
				
				if(sizeChanged)
					this.OnWindowSizeChanged();
			}
		}

		/// <summary>
		/// Sets the screen using its index.
		/// </summary>
		/// <value>
		/// The index of the screen (0 is the primary screen, 1 the secondary, etc ...).
		/// </value>
		public int ScreenIndex
		{
			set
			{
				if (value < 0 || value > MaxScreenIndex)
				{
					throw new IndexOutOfRangeException("Screen with index " + value + " does not exist. Check PresentationArea.MaxScreenIndex.");
				}

				this.Screen = Screen.AllScreens[value];
			}
		}

		/// <summary>
		/// Gets the maximum screen index.
		/// </summary>
		/// <value>
		/// The maximum screen index. This is the number of screen minus one.
		/// </value>
		public static int MaxScreenIndex
		{
			get
			{
				return Screen.AllScreens.Length - 1;
			}
		}
		
		private Point offset;
		
		/// <summary>
		/// Gets or sets the offset from the top left corner of the screen. This is ignored in fullscreen mode.
		/// </summary>
		public Point Offset
		{
			get
			{
				return offset;
			}
			set
			{
				if(value.X > this.Screen.Bounds.Width - 10 ||
				   value.Y > this.Screen.Bounds.Height - 10 ||
				   value.X < -this.Size.Width + 10 ||
				   value.Y < -this.Size.Height + 10)
					throw new ArgumentException("New offset is outside of screen");
				
				offset = value;
				
				if(!this.Fullscreen)
					OnWindowLocationChanged();
			}
		}
		
		/// <summary>
		/// Gets or sets the offset from the left of the screen. This is ignored in fullscreen mode.
		/// </summary>
		public int OffsetLeft
		{
			get
			{
				return Offset.X;
			}
			set
			{
				Offset = new Point(value, Offset.Y);
			}
		}
		
		/// <summary>
		/// Gets or sets the offset from the top of the screen. This is ignored in fullscreen mode.
		/// </summary>
		public int OffsetTop
		{
			get
			{
				return Offset.Y;
			}
			set
			{
				Offset = new Point(Offset.X, value);
			}
		}
		
		private Size size;
		
		/// <summary>
		/// Gets or sets the size of the PresentationArea. This is ignored in fullscreen mode.
		/// </summary>
		public Size Size
		{
			get
			{
				return size;
			}
			set
			{
				if(value.Height < 10 || value.Width < 10)
					throw new ArgumentException("Size must be at least 10x10");
				size = value;
				
				if(!this.Fullscreen)
					OnWindowSizeChanged();
			}
		}
		
		/// <summary>
		/// Gets or sets the width of the PresentationArea. This is ignored in fullscreen mode.
		/// </summary>
		public int Width
		{
			get
			{
				return this.Size.Width;
			}
			set
			{
				this.Size = new Size(value, this.Size.Height);
			}
		}
		
		/// <summary>
		/// Gets or sets the height of the PresentationArea. This is ignored in fullscreen mode.
		/// </summary>
		public int Height
		{
			get
			{
				return this.Size.Height;
			}
			set
			{
				this.Size = new Size(this.Size.Width, value);
			}
		}
		
		private bool fullscreen;
		
		/// <summary>
		/// Gets or sets a value indicating whether the PresentationArea uses the full screen.
		/// </summary>
		public bool Fullscreen
		{
			get
			{
				return fullscreen;
			}
			set
			{
				if (fullscreen != value)
				{
					fullscreen = value;
					
					if (fullscreen)
					{
						if (Offset.X != 0 || Offset.Y != 0)
							OnWindowLocationChanged();
						
						OnWindowSizeChanged();
					}
					else
					{
						OnWindowSizeChanged();
						
						if (Offset.X != 0 || Offset.Y != 0)
							OnWindowLocationChanged();
					}
				}
			}
		}
		
		/// <summary>
		/// Gets the size the window should have to fit this PresentationArea.
		/// </summary>
		public Size WindowSize
		{
			get
			{
				if(this.Fullscreen)
					return this.Screen.Bounds.Size;
				else
					return size;
			}
		}
		
		/// <summary>
		/// Gets the location, where the window should be positioned to fit this PresentationArea.
		/// </summary>
		public Point WindowLocation
		{
			get
			{
				if(this.Fullscreen)
					return this.Screen.Bounds.Location;
				else
					return new Point(this.Screen.Bounds.Location.X + this.Offset.X, this.Screen.Bounds.Location.Y + this.Offset.Y);
			}
		}
	}
}
