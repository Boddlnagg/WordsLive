using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Presentation.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Words.Slideshow
{
    public class SlideshowPreviewProvider : WpfPreviewProvider
    {
        private Image image = new Image();

        public SlideshowPreviewProvider(SlideshowPresentationBase presentation)
        {
            // assume that presentation has already been loaded

            presentation.SlideIndexChanged += (sender, args) =>
            {
                image.Dispatcher.Invoke(new Action(() => { image.Source = presentation.Thumbnails[presentation.SlideIndex]; }));
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
