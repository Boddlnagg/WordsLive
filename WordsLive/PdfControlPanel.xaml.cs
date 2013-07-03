using System;
using System.Windows.Controls;
using WordsLive.Awesomium;
using WordsLive.Core;

namespace WordsLive
{
	/// <summary>
	/// Interaktionslogik für PdfControlPanel.xaml
	/// </summary>
	[TargetMedia(typeof(PdfMedia))]
	public partial class PdfControlPanel : UserControl, IMediaControlPanel
	{
		private PdfPresentation presentation;
		private PdfMedia media;

		public PdfControlPanel()
		{
			InitializeComponent();
		}

		public Control Control
		{
			get { return this; }
		}

		public Media Media
		{
			get { return media; }
		}

		public void Init(Media media)
		{
			if (!(media is PdfMedia))
				throw new ArgumentException("media must be of type PdfMedia");

			this.media = media as PdfMedia;

			if (!media.Uri.IsFile)
				throw new NotImplementedException("Loading remote URIs not implemented yet.");

			presentation = Controller.PresentationManager.CreatePresentation<PdfPresentation>();
			presentation.Load(this.media);
			Controller.PresentationManager.CurrentPresentation = presentation;
		}

		public bool IsUpdatable
		{
			get { return false; } // TODO
		}

		public void Close()
		{
			//presentation.Close();
		}

		public ControlPanelLoadState LoadState
		{
			get { return ControlPanelLoadState.Loaded; }
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			presentation.GotoPage(int.Parse(pageTextBox.Text));
		}

		private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
		{
			presentation.PreviousPage();
		}

		private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
		{
			presentation.NextPage();
		}

		private void Button_Click_3(object sender, System.Windows.RoutedEventArgs e)
		{
			presentation.FitToWidth();
		}

		private void Button_Click_4(object sender, System.Windows.RoutedEventArgs e)
		{
			presentation.WholePage();
		}
	}
}
