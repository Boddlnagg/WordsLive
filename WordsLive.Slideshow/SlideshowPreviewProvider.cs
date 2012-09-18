using System;
using System.Windows;
using System.Windows.Controls;
using WordsLive.Presentation.Wpf;

namespace WordsLive.Slideshow
{
    public class SlideshowPreviewProvider : WpfPreviewProvider
    {
        private Image image = new Image();

        public SlideshowPreviewProvider(SlideshowPresentationBase presentation)
        {
            // assume that presentation has already been loaded

            presentation.SlideIndexChanged += (sender, args) =>
            {
                image.Dispatcher.Invoke(new Action(() => { image.Source = presentation.Thumbnails[presentation.SlideIndex].Image; }));
            };
        }

        protected override UIElement WpfControl
        {
            get
            {
                return image;
            }
        }
    }
}
