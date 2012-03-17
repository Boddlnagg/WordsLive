using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Presentation.Wpf;
using System.Windows.Controls;

namespace Words.AudioVideo
{
	public class AudioVideoPresentation<T> : WpfPresentation<T>, IAudioVideoPresentation where T : BaseMediaControl, new()
	{
		public BaseMediaControl MediaControl
		{
			get
			{
				return Control;
			}
		}

		public override void Close()
		{
			Control.Destroy();
			base.Close();
		}
	}
}
