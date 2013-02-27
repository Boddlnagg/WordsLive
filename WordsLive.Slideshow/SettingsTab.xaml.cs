using System.Windows;
using System.Windows.Controls;
using WordsLive.Slideshow.Resources;

namespace WordsLive.Slideshow
{
	/// <summary>
	/// Interaktionslogik für SettingsTab.xaml
	/// </summary>
	public partial class SettingsTab : UserControl, ISettingsTab
	{
		public SettingsTab()
		{
			InitializeComponent();
			this.DataContext = this;
		}

		public bool EnableLivePreview
		{
			get
			{
				return Settings.EnableLivePreview;
			}
			set
			{
				Settings.EnableLivePreview = value;
			}
		}

		public FrameworkElement Control
		{
			get { return this; }
		}

		public string Header
		{
			get
			{
				return Resource.seHeader;
			}
		}
	}
}
