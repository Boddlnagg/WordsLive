using System;
using System.Windows;
using System.Windows.Controls;
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
			presentation = Controller.PresentationManager.CreatePresentation<PdfPresentation>();
			presentation.Load(false);
			presentation.Control.Web.JSConsoleMessageAdded += (sender, args) => MessageBox.Show(args.Message);
			presentation.Control.Web.LoadFile(@"pdf.html");
			Controller.PresentationManager.CurrentPresentation = presentation;
		}

		public bool IsUpdatable
		{
			get { return false; } // TODO
		}

		public void Close()
		{
			presentation.Close();
		}

		public ControlPanelLoadState LoadState
		{
			get { return ControlPanelLoadState.Loaded; }
		}
	}
}
