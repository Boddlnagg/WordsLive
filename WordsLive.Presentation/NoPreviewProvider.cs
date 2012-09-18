using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordsLive.Presentation
{
	/// <summary>
	/// An implementation of IPreviewProvider that simply provides no preview.
	/// Use this whenever a presentation is unable to provide a preview.
	/// </summary>
	public class NoPreviewProvider : IPreviewProvider
	{
		/// <summary>
		/// Gets the control that contains the preview.
		/// In this case, <c>null</c> is returned, as there is no preview.
		/// </summary>
		public System.Windows.Forms.Control PreviewControl
		{
			get
			{
				return null;
			}
		}

		public bool IsPreviewAvailable
		{
			get { return false; }
		}
	}
}
