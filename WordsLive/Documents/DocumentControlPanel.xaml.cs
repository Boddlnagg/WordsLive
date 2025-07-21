/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

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
					presentation.GoToPage(value);
				}
			}
		}

		public DocumentPageScale PageScale
		{
			get
			{
				return media.PageScale;
			}
			set
			{
				if (media.PageScale != value)
				{
					media.PageScale = value;

					presentation.PageScale = value;

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
			get { return false; } // TODO: make it improve anything to make this updatable?
		}

		bool closed = false;

		public void Close()
		{
			closed = true;

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
			if (e.Command == NavigationCommands.PreviousPage)
			{
				e.CanExecute = LoadState == ControlPanelLoadState.Loaded && !closed && presentation.CanGoToPreviousPage;
				e.Handled = true;
			}
			else if (e.Command == NavigationCommands.NextPage)
			{
				e.CanExecute = LoadState == ControlPanelLoadState.Loaded && !closed && presentation.CanGoToNextPage;
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

			if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.PageDown || e.Key == Key.Space)
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
