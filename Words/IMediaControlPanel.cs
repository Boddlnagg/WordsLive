using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Words.Core;

namespace Words
{
	public interface IMediaControlPanel
	{
		Control Control { get; }
		Media Media { get; }
		void Init(Media media);

		/// <summary>
		/// Whether or not this ControlPanel can be updated by calling Init() again
		/// </summary>
		bool IsUpdatable { get; }
		void Close();
		ControlPanelLoadState LoadState { get; }
	}
}
