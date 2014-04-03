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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace WordsLive.Utils
{
	/// <summary>
	/// RoutedCommand that has a localized version of the input gesture text.
	/// It must be manually bound to the InputGestureText property of menu items.
	/// </summary>
	public class LocalizedRoutedCommand : RoutedCommand
	{
		public LocalizedRoutedCommand() : base() { }

		public LocalizedRoutedCommand(string name, Type ownerType) : base(name, ownerType) { }

		public LocalizedRoutedCommand(string name, Type ownerType, InputGestureCollection inputGestures) : base(name, ownerType, inputGestures) { }

		public string GestureText
		{
			get
			{
				var keyGesture = InputGestures.OfType<KeyGesture>().FirstOrDefault();
				
				if (keyGesture == null)
					return null;

				var text = keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
				if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("de", StringComparison.InvariantCultureIgnoreCase))
					return text.Replace("Ctrl", "Strg");
				else
					return text;
			}
		}
	}
}
