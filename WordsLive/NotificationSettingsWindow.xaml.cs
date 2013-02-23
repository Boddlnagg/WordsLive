using System.Windows;

namespace WordsLive
{
	/// <summary>
	/// Interaktionslogik für NotificationSettingsWindow.xaml
	/// </summary>
	public partial class NotificationSettingsWindow : Window
	{
		public NotificationSettingsWindow()
		{
			InitializeComponent();
			DataContext = NotificationController.Instance;
		}

		private void Show_Click(object sender, RoutedEventArgs e)
		{
			NotificationController.Instance.Show();
		}

		private void Hide_Click(object sender, RoutedEventArgs e)
		{
			NotificationController.Instance.Hide();
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		// TODO: localization, change label of show/update button dynamically, improve UI layout
	}
}
