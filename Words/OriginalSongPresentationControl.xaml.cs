using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Words.Core.Songs;
using Words.Core;

namespace WpfTest
{
    
	/// <summary>
	/// Interaktionslogik für SongPresentation.xaml
	/// </summary>
	public partial class OriginalSongPresentationControl : UserControl
	{
        //private SongSlide slide = new SongSlide(1280,1024);
        //public OriginalSongPresentationControl()
        //{
        //    this.InitializeComponent();
        //    this.DataContext = slide;
        //    this.SizeChanged += delegate{slide.UpdateSize(this.ActualWidth, this.ActualHeight); };
        //}

        //private Song song;
        //public Song Song
        //{
        //    get
        //    {
        //        return song;
        //    }
        //    set
        //    {
        //        song = value;
        //        if (song != null)
        //            this.BackgroundImage.Source = new BitmapImage(new Uri(MediaManager.BackgroundsDirectory + song.Background.ImagePath)); // TODO: use binding
        //    }
        //}
	}
}