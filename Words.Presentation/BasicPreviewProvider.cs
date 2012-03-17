using System;
using System.Windows.Forms;

namespace Words.Presentation
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
