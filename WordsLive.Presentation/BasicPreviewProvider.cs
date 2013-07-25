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

using System.Windows.Forms;

namespace WordsLive.Presentation
{
	/// <summary>
	/// A basic preview provider, that provide a preview using a WinForms control.
	/// </summary>
	/// <typeparam name="T">The type of the WinForms control.</typeparam>
	public class BasicPreviewProvider<T> : IPreviewProvider where T : Control
	{
		private T control;

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicPreviewProvider&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="control">The preview control.</param>
		public BasicPreviewProvider(T control)
		{
			this.control = control;
		}

		/// <summary>
		/// Gets the control that contains the preview.
		/// </summary>
		public Control PreviewControl
		{
			get
			{
				return control;
			}
		}

		/// <summary>
		/// Gets the control that contains the preview and keeps its type.
		/// </summary>
		public T Control
		{
			get
			{
				return control;
			}
		}

		public bool IsPreviewAvailable
		{
			get { return true; }
		}
	}
}
