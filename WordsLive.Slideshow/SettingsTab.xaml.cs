using System.Windows;
using System.Windows.Controls;
using WordsLive.Slideshow.Resources;

namespace WordsLive.Slideshow
{
	public partial class SettingsTab : UserControl/*, ISettingsTab*/
	{
		public SettingsTab()
		{
			InitializeComponent();
			this.DataContext = this;
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
