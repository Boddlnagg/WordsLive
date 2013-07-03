using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WordsLive.Core;

namespace WordsLive
{
	/// <summary>
	/// Interaktionslogik für PdfControlPanel.xaml
	/// </summary>
	[TargetMedia(typeof(PdfMedia))]
	public partial class PdfControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		private PdfPresentation presentation;
		private PdfMedia media;
		private ControlPanelLoadState loadState = ControlPanelLoadState.Loading;
		private DocumentPageScale pageScale = DocumentPageScale.FitToWidth;

		public PdfControlPanel()
		{
			InitializeComponent();
			this.DataContext = this;
		}

		public Control Control
		{
			get { return this; }
		}

		public Media Media
		{
			get { return media; }
		}

		public string FormattedPageCount
		{
			get
			{
				return presentation.IsLoaded ? presentation.PageCount.ToString() : "-";
			}
		}

		public int CurrentPage
		{
			get
			{
				return presentation.IsLoaded ? presentation.CurrentPage : 1;
			}
			set
			{
				if (value != presentation.CurrentPage)
				{
					presentation.GotoPage(value);
				}
			}
		}

		public DocumentPageScale PageScale
		{
			get
			{
				return pageScale;
			}
			set
			{
				if (pageScale != value)
				{
					pageScale = value;

					if (pageScale == DocumentPageScale.FitToWidth)
						presentation.FitToWidth();
					else
						presentation.WholePage();

					OnPropertyChanged("PageScale");
					OnPropertyChanged("CurrentPage");
				}
			}
		}

		public void Init(Media media)
		{
			if (!(media is PdfMedia))
				throw new ArgumentException("media must be of type PdfMedia");

			this.media = media as PdfMedia;

			if (!media.Uri.IsFile)
				throw new NotImplementedException("Loading remote URIs not implemented yet.");

			presentation = Controller.PresentationManager.CreatePresentation<PdfPresentation>();
			presentation.DocumentLoaded += (sender, args) =>
			{
				LoadState = ControlPanelLoadState.Loaded;
				OnPropertyChanged("FormattedPageCount");
			};
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
			get
			{
				return loadState;
			}
			private set
			{
				loadState = value;
				OnPropertyChanged("LoadState");
			}
		}

		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.PreviousPage || e.Command == NavigationCommands.NextPage)
			{
				e.CanExecute = LoadState == ControlPanelLoadState.Loaded;
				e.Handled = true;
			}
		}

		private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == NavigationCommands.PreviousPage)
			{
				presentation.PreviousPage();
				OnPropertyChanged("CurrentPage");
			}
			else if (e.Command == NavigationCommands.NextPage)
			{
				presentation.NextPage();
				OnPropertyChanged("CurrentPage");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var textBox = sender as TextBox;
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				BindingExpression exp = textBox.GetBindingExpression(TextBox.TextProperty);
				exp.UpdateSource();
			}
		}
	}

	public enum DocumentPageScale
	{
		FitToWidth,
		WholePage
	}
}
