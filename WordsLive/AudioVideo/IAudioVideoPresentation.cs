using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Presentation;
using WordsLive.Presentation.Wpf;

namespace WordsLive.AudioVideo
{
	interface IAudioVideoPresentation : IWpfPresentation
	{
		BaseMediaControl MediaControl { get; }
	}
}
