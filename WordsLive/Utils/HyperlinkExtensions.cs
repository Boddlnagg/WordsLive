/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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

using System.Windows;
using System.Windows.Documents;
using WordsLive.Core;

namespace WordsLive.Utils
{
	/// <summary>
	/// Register an attached property to open external hyperlinks in the default browser.
	/// Taken from http://stackoverflow.com/a/11433814
	/// </summary>
	public static class HyperlinkExtensions
	{
		public static bool GetIsExternal(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsExternalProperty);
		}

		public static void SetIsExternal(DependencyObject obj, bool value)
		{
			obj.SetValue(IsExternalProperty, value);
		}
		public static readonly DependencyProperty IsExternalProperty =
			DependencyProperty.RegisterAttached("IsExternal", typeof(bool), typeof(HyperlinkExtensions), new UIPropertyMetadata(false, OnIsExternalChanged));

		private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			var hyperlink = sender as Hyperlink;

			if ((bool)args.NewValue)
				hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
			else
				hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
		}

		private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			e.Uri.OpenInBrowser();
			e.Handled = true;
		}
	}
}
