using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WordsLive.Core;

namespace WordsLive.Documents
{
	[TargetMedia(typeof(DocumentMedia))]
	public partial class DocumentControlPanel : UserControl, IMediaControlPanel, INotifyPropertyChanged
	{
		private IDocumentPresentation presentation;
		private DocumentMedia media;
		private ControlPanelLoadState loadState = ControlPanelLoadState.Loading;
		private DocumentPageScale pageScale = DocumentPageScale.FitToWidth;

		public DocumentControlPanel()
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
			if (!(media is DocumentMedia))
				throw new ArgumentException("media must be of type DocumentMedia");

			this.media = media as DocumentMedia;

			if (!media.Uri.IsFile)
				throw new NotImplementedException("Loading remote URIs not implemented yet.");

			presentation = this.media.CreatePresentation();
			presentation.DocumentLoaded += (sender, args) =>
			{
				LoadState = ControlPanelLoadState.Loaded;
				Controller.PresentationManager.CurrentPresentation = presentation;
				OnPropertyChanged("FormattedPageCount");
			};
			presentation.Load();
		}

		public bool IsUpdatable
		{
			get { return false; } // TODO
		}

		public void Close()
		{
			if (presentation != Controller.PresentationManager.CurrentPresentation)
				presentation.Close();
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

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.PageDown)
			{
				presentation.NextPage();
				OnPropertyChanged("CurrentPage");
				e.Handled = true;
			}
			else if (e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.PageUp)
			{
				presentation.PreviousPage();
				OnPropertyChanged("CurrentPage");
				e.Handled = true;
			}
		}
	}
}
