/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
