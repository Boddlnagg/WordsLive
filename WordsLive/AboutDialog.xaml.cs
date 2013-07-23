using System.Reflection;
using System.Windows;
using WordsLive.Utils;

namespace WordsLive
{
	public partial class AboutDialog : Window
	{
		public AboutDialog()
		{
			InitializeComponent();

			this.DataContext = this;
		}

		public string VersionString
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.SimplifyVersion().ToString();
			}
		}
	}
}
