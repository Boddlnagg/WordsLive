using System;
using System.Windows.Forms;

namespace Words.Presentation
{
	public interface IPreviewProvider
	{
		/// <summary>
		/// Gets a value indicating whether there is a preview available.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if there is a preview available; otherwise, <c>false</c>.
		/// </value>
		bool IsPreviewAvailable { get; }

		/// <summary>
		/// Gets the control that contains the preview.
		/// </summary>
		Control PreviewControl { get; }
	}
}
