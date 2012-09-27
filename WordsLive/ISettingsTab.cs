using System.Windows;

namespace WordsLive
{
	public interface ISettingsTab
	{
		FrameworkElement Control { get; }
		string Header { get; }
	}
}
