using System;
using System.Reflection;
using System.Windows;

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
				var a = Assembly.GetExecutingAssembly();
				Version appVersion = a.GetName().Version;
				return appVersion.Major + "." + appVersion.Minor + "." + appVersion.Build;
			}
		}
	}
}
