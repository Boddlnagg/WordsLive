using System;
using System.Drawing;
using System.Windows.Forms;

namespace WordsLive.Presentation
{
	/// <summary>
	/// Implements a form that has an assigned presentation area and resizes the window accordingly.
	/// </summary>
	public partial class PresentationForm : Form
	{
		public PresentationForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(0, 0);
			this.TopMost = true;
			this.ShowInTaskbar = false;
			
			this.Area = new PresentationArea();
		}
		
		private PresentationArea area;
		
		public PresentationArea Area
		{
			get
			{
				return this.area;
			}
			set
			{
				if (this.area != value)
				{
					this.area = value;
					this.Location = this.area.WindowLocation;
					this.Size = this.area.WindowSize;
					this.area.WindowSizeChanged += delegate { this.Size = this.area.WindowSize; };
					this.area.WindowLocationChanged += delegate { this.Location = this.area.WindowLocation; };
				}
			}
		}
	}
}
