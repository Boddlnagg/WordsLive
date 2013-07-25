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

using System.Windows;
using System.Windows.Forms.Integration;
using WinForms = System.Windows.Forms;

namespace WordsLive.Presentation.Wpf
{
	/// <summary>
	/// Abstract base class for preview providers that use an WPF UIElement to display the preview.
	/// </summary>
	public abstract class WpfPreviewProvider : IPreviewProvider
	{
		private ElementHost cachedHost;

		public WinForms.Control PreviewControl
		{
			get
			{
				if (WpfControl == null)
					return null;

				if (cachedHost == null)
				{
					this.cachedHost = new ElementHost();
					this.cachedHost.Dock = System.Windows.Forms.DockStyle.Fill;
					this.cachedHost.Child = WpfControl;
				}

				return this.cachedHost;
			}
		}

		public UIElement WpfPreviewControl
		{
			get
			{
				if (cachedHost != null)
				{
					cachedHost.Child = null; // this prevents the control from being child to two visual parents
					cachedHost = null;
				}

				return WpfControl;
			}
		}

		public bool IsPreviewAvailable
		{
			get { return WpfControl != null; }
		}

		protected abstract UIElement WpfControl { get; }
	}
}

