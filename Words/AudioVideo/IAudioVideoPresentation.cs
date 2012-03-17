using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Presentation;
using Words.Presentation.Wpf;

namespace Words.AudioVideo
{
	interface IAudioVideoPresentation : IWpfPresentation
	{
		BaseMediaControl MediaControl { get; }
	}
}
